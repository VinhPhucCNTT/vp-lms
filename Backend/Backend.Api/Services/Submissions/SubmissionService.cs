using Backend.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Services.Common;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Content;
using AutoMapper;
using Backend.Persistence.Entities.Assignments;
using Backend.Api.Core.Common;
using ByteSizeLib;
using Sqids;
using Backend.Api.Services.Content;

namespace Backend.Api.Services.Submissions;

public class SubmissionService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    FileService fileService,
    SqidsEncoder<long> sqidsEncoder,
    IMapper mapper)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly FileService _fileService = fileService;
    private readonly SqidsEncoder<long> _sqidsEncoder = sqidsEncoder;
    private readonly IMapper _mapper = mapper;

    public async Task<PaginatedResponse<SubmissionResponse>> GetListAsync(long resourceId, PageRequest page, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var submissions = db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => s.Assignment.ResourceId == resourceId)
            .Select(s => _mapper.Map<SubmissionResponse>(s));

        var list = await submissions
        .Skip((page.PageNumber - 1) * page.PageSize)
        .Take(page.PageSize)
        .ToListAsync(ct);

        return new PaginatedResponse<SubmissionResponse>(
            page.PageNumber,
            page.PageSize,
            await submissions.CountAsync(ct),
            list);
    }

    public async Task<Result<SubmissionDetailResponse>> GetOwnDetailAsync(long resourceId, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var assignment = await GetStudentAssignmentAsync(db, resourceId, ct);
        if (assignment is null)
            return Result<SubmissionDetailResponse>.Failure(ErrorType.NotFound, "Assignment not found.");

        var userId = _currentUserService.UserId;
        var submission = await db.AssignmentSubmissions
            .AsNoTracking()
            .Include(s => s.Grade)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.UserId == userId, ct);
        if (submission is null)
            return Result<SubmissionDetailResponse>.Success(
                CreateSubmissionDetail(assignment, userId, null, []));

        var files = await db.AssignmentFiles
            .AsNoTracking()
            .Include(f => f.File)
            .Where(f => f.SubmissionId == submission.Id)
            .OrderBy(f => f.OrderIndex)
            .Select(f => _mapper.Map<FileResponse>(f.File))
            .ToArrayAsync(ct);

        return Result<SubmissionDetailResponse>.Success(
            CreateSubmissionDetail(assignment, userId, submission, files));
    }

    // NOTE: Probably don't need this?
    // public async Task<SubmissionDetailResponse?> GetByUserIdAsync(long resourceId, long userId, CancellationToken ct = default)
    // {
    //     using var db = await _dbFactory.CreateDbContextAsync(ct);
    //     return await db.AssignmentSubmissions
    //         .AsNoTracking()
    //         .Where(s => s.AssignmentId == resourceId && s.UserId == userId)
    //         .Select(s => _mapper.Map<SubmissionDetailResponse>(s))
    //         .FirstOrDefaultAsync(ct);
    // }

    public async Task<PaginatedResponse<SubmissionResponse>> GetGradedAsync(long resourceId, PageRequest page, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);

        var submissions = db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == resourceId && s.Grade != null)
            .Select(s => _mapper.Map<SubmissionResponse>(s));

        var list = await submissions
        .Skip((page.PageNumber - 1) * page.PageSize)
        .Take(page.PageSize)
        .ToListAsync(ct);

        return new PaginatedResponse<SubmissionResponse>(
            page.PageNumber,
            page.PageSize,
            await submissions.CountAsync(ct),
            list);
    }

    public async Task<PaginatedResponse<SubmissionResponse>> GetUngradedAsync(long resourceId, PageRequest page, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);

        var submissions = db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == resourceId && s.Grade == null)
            .Select(s => _mapper.Map<SubmissionResponse>(s));

        var list = await submissions
        .Skip((page.PageNumber - 1) * page.PageSize)
        .Take(page.PageSize)
        .ToListAsync(ct);

        return new PaginatedResponse<SubmissionResponse>(
            page.PageNumber,
            page.PageSize,
            await submissions.CountAsync(ct),
            list);
    }

    public async Task<Result<SubmissionDetailResponse>> SubmitAsync(long resourceId, SubmissionRequest request, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var userId = _currentUserService.UserId;
        var assignment = await GetStudentAssignmentAsync(db, resourceId, ct);
        if (assignment is null)
            return Result<SubmissionDetailResponse>.Failure(ErrorType.NotFound, "Assignment not found");

        var availabilityError = ValidateAvailability(assignment);
        if (availabilityError is not null)
            return Result<SubmissionDetailResponse>.Failure(ErrorType.Validation, availabilityError);

        var submission = await db.AssignmentSubmissions
            .Include(s => s.Grade)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.UserId == userId, ct);

        if (submission?.SubmittedOn is not null)
            return Result<SubmissionDetailResponse>.Success(
                await LoadSubmissionDetailAsync(db, assignment, submission, userId, ct));

        var text = request.SubmissionText?.Trim();
        var textError = ValidateSubmissionText(assignment, text);
        if (textError is not null)
            return Result<SubmissionDetailResponse>.Failure(ErrorType.Validation, textError);

        var fileCount = submission is null
            ? 0
            : await db.AssignmentFiles.CountAsync(f => f.SubmissionId == submission.Id, ct);

        if ((assignment.SubmissionType is SubmissionType.File or SubmissionType.Both) && fileCount == 0 && string.IsNullOrWhiteSpace(text))
            return Result<SubmissionDetailResponse>.Failure(ErrorType.Validation, "Add at least one file or text response before submitting.");

        if (submission is null)
            submission = await CreateEmptySubmissionAsync(db, assignment.Id, userId, ct);

        submission.SubmissionText = text;
        submission.SubmittedOn = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Result<SubmissionDetailResponse>.Success(
            await LoadSubmissionDetailAsync(db, assignment, submission, userId, ct));
    }

    public async Task<AssignmentFileResponse> UploadFileAsync(AssignmentSubmission submission, IFormFile file, CancellationToken ct)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        await using var stream = file.OpenReadStream();
        var userId = _currentUserService.UserId;

        var fileId = await _fileService.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            userId,
            FileCategory.AssignmentSubmission,
            ct);

        int fileCount = await db.AssignmentFiles.AsNoTracking()
            .CountAsync(f => f.SubmissionId == submission.Id, ct);
        var assignmentFile = new AssignmentFile
        {
            SubmissionId = submission.Id,
            FileId = fileId,
            OrderIndex = fileCount
        };
        db.AssignmentFiles.Add(assignmentFile);
        await db.SaveChangesAsync(ct);

        var resourceId = await db.Assignments
            .Where(a => a.Id == submission.AssignmentId)
            .Select(a => a.ResourceId)
            .SingleAsync(ct);

        var fileAsset = await db.FileAssets
            .AsNoTracking()
            .SingleAsync(f => f.Id == fileId, ct);

        return new AssignmentFileResponse(
            _sqidsEncoder.Encode(resourceId),
            _mapper.Map<FileResponse>(fileAsset));
    }

    public async Task<Result<AssignmentSubmission>> ValidateFileAsync(
        long resourceId,
        IFormFile file,
        CancellationToken ct)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var userId = _currentUserService.UserId;

        var assignment = await GetStudentAssignmentAsync(db, resourceId, ct);
        if (assignment is null) return Result<AssignmentSubmission>.Failure(ErrorType.NotFound, "Assignment not found");

        var submission = await db.AssignmentSubmissions
            .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.UserId == userId, ct);

        var availabilityError = ValidateAvailability(assignment);
        if (availabilityError is not null)
            return Result<AssignmentSubmission>.Failure(ErrorType.Validation, availabilityError);

        if (submission?.SubmittedOn is not null)
            return Result<AssignmentSubmission>.Failure(ErrorType.Validation, "This assignment has already been submitted.");

        if (assignment.SubmissionType == SubmissionType.Text)
            return Result<AssignmentSubmission>.Failure(ErrorType.Validation, "Assignment does not accept files.");

        if (assignment.AllowedExtensions is not null)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!assignment.AllowedExtensions.Any(x => string.Equals(x, extension, StringComparison.OrdinalIgnoreCase)))
                return Result<AssignmentSubmission>.Failure(ErrorType.Validation, $"This assignment does not accept extension of type {extension} (allowed: {assignment.AllowedExtensions}).");
        }

        var maxFileSize = ByteSize.FromKiloBytes(assignment.MaxFileSizeKb);
        if (file.Length > maxFileSize.Bytes)
            return Result<AssignmentSubmission>.Failure(ErrorType.Validation, $"File is too large ({file.Length} > {assignment.MaxFileSizeKb} Kb).");

        if (assignment.MaxFileCount is not null)
        {
            var currentFileCount = submission is null
                ? 0
                : await db.AssignmentFiles.AsNoTracking().CountAsync(f => f.SubmissionId == submission.Id, ct);
            if (currentFileCount >= assignment.MaxFileCount)
                return Result<AssignmentSubmission>.Failure(ErrorType.Validation, $"File count exceeded (max is {assignment.MaxFileCount}).");
        }

        submission ??= await CreateEmptySubmissionAsync(db, assignment.Id, userId, ct);
        return Result<AssignmentSubmission>.Success(submission);
    }

    public async Task<bool> RemoveAsync(long resourceId, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var userId = _currentUserService.UserId;

        var submission = await db.AssignmentSubmissions.FirstOrDefaultAsync(
            s => s.Assignment.ResourceId == resourceId && s.UserId == userId,
            ct);
        if (submission is null) return false;
        if (!await CanStudentAccessAsync(db, resourceId, userId, ct))
            return false;

        var files = await db.AssignmentFiles.Where(f => f.SubmissionId == submission.Id).ToListAsync(ct);
        foreach (var file in files)
        {
            db.AssignmentFiles.Remove(file);
            await _fileService.DeleteAsync(file.FileId, ct);
        }
        db.AssignmentSubmissions.Remove(submission);
        await db.SaveChangesAsync(ct);

        return true;
    }

    private static async Task<AssignmentSubmission> CreateEmptySubmissionAsync(AppDbContext db, long assignmentId, long userId, CancellationToken ct = default)
    {
        var submission = new AssignmentSubmission
        {
            AssignmentId = assignmentId,
            UserId = userId,
            SubmissionText = null,
            SubmittedOn = null
        };
        db.AssignmentSubmissions.Add(submission);
        await db.SaveChangesAsync(ct);
        return submission;
    }

    private static async Task<bool> CanStudentAccessAsync(AppDbContext db, long resourceId, long userId, CancellationToken ct = default)
    {
        var assignment = await db.Assignments.FirstOrDefaultAsync(a => a.ResourceId == resourceId, ct);

        if (assignment is null)
            return false;

        var currentDate = DateTime.Now;
        if (assignment.OpenDate is not null && currentDate < assignment.OpenDate)
            return false;
        if (assignment.CloseDate is not null && currentDate > assignment.CloseDate)
            return false;

        var submission = await db.AssignmentSubmissions.FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.UserId == userId, ct);
        if (submission is null) return false;
        if (await db.AssignmentGrades.AsNoTracking().AnyAsync(g => g.SubmissionId == submission.Id, ct))
            return false;

        return true;
    }

    private async Task<Assignment?> GetStudentAssignmentAsync(
        AppDbContext db,
        long resourceId,
        CancellationToken ct)
    {
        var userId = _currentUserService.UserId;
        return await db.Assignments
            .Include(a => a.Resource)
                .ThenInclude(r => r.Module)
            .Where(a => a.ResourceId == resourceId && a.Resource.IsPublished)
            .Where(a => db.Enrollments.Any(e =>
                e.CourseId == a.Resource.Module.CourseId &&
                e.UserId == userId))
            .FirstOrDefaultAsync(ct);
    }

    private static string? ValidateAvailability(Assignment assignment)
    {
        var now = DateTime.UtcNow;
        if (assignment.OpenDate is not null && now < assignment.OpenDate)
            return "This assignment is not available yet.";
        if (assignment.CloseDate is not null && now >= assignment.CloseDate)
            return "This assignment is closed for submissions.";
        return null;
    }

    private static string? ValidateSubmissionText(Assignment assignment, string? text)
    {
        if (assignment.SubmissionType == SubmissionType.File && !string.IsNullOrWhiteSpace(text))
            return "This assignment accepts files only.";

        if (assignment.SubmissionType == SubmissionType.Text && string.IsNullOrWhiteSpace(text))
            return "A text response is required.";

        var length = text?.Length ?? 0;
        if (assignment.MinTextLength is not null && length < assignment.MinTextLength)
            return $"The text response must be at least {assignment.MinTextLength} characters.";
        if (assignment.MaxTextLength is not null && length > assignment.MaxTextLength)
            return $"The text response must be at most {assignment.MaxTextLength} characters.";

        return null;
    }

    private async Task<SubmissionDetailResponse> LoadSubmissionDetailAsync(
        AppDbContext db,
        Assignment assignment,
        AssignmentSubmission submission,
        long userId,
        CancellationToken ct)
    {
        var files = await db.AssignmentFiles
            .AsNoTracking()
            .Include(f => f.File)
            .Where(f => f.SubmissionId == submission.Id)
            .OrderBy(f => f.OrderIndex)
            .Select(f => _mapper.Map<FileResponse>(f.File))
            .ToArrayAsync(ct);

        return CreateSubmissionDetail(assignment, userId, submission, files);
    }

    private SubmissionDetailResponse CreateSubmissionDetail(
        Assignment assignment,
        long userId,
        AssignmentSubmission? submission,
        FileResponse[] files) => new(
            _sqidsEncoder.Encode(assignment.Id),
            _sqidsEncoder.Encode(userId),
            submission?.SubmissionText,
            submission?.SubmittedOn,
            submission?.Grade is not null
                ? "graded"
                : submission?.SubmittedOn is not null
                    ? "submitted"
                    : "not-submitted",
            submission?.Grade?.Score,
            submission?.Grade?.FeedbackText,
            files);
}
