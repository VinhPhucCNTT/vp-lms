using Backend.Api.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Services.Common;
using Backend.Api.Core.Types;
using Backend.Api.Core.Entities.Content;
using AutoMapper;
using Backend.Api.Core.Entities.Assignments;
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
        var assignment = await db.Assignments.AsNoTracking().FirstOrDefaultAsync(a => a.ResourceId == resourceId, ct);
        if (assignment is null)
            return Result<SubmissionDetailResponse>.Failure(ErrorType.NotFound, "Assignment not found.");

        var userId = _currentUserService.UserId;
        var submission = await db.AssignmentSubmissions.AsNoTracking().FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.UserId == userId, ct);
        if (submission is null)
            return Result<SubmissionDetailResponse>.Success(new(
                _sqidsEncoder.Encode(assignment.Id),
                _sqidsEncoder.Encode(userId),
                null, []));

        FileResponse[] files = await db.AssignmentFiles.AsNoTracking()
            .Where(f => f.SubmissionId == submission.Id)
            .Select(f => _mapper.Map<FileResponse>(f))
            .ToArrayAsync(ct);

        return Result<SubmissionDetailResponse>.Success(new(
            _sqidsEncoder.Encode(assignment.Id),
            _sqidsEncoder.Encode(userId),
            submission.SubmissionText, files));
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
        var assignment = await db.Assignments.FirstOrDefaultAsync(a => a.ResourceId == resourceId, ct);
        var userId = _currentUserService.UserId;
        if (assignment is null)
            return Result<SubmissionDetailResponse>.Failure(ErrorType.NotFound, "Assignment not found");

        var submission =
            await db.AssignmentSubmissions.FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.UserId == userId, ct)
            ?? await CreateEmptySubmissionAsync(db, assignment.Id, userId, ct);

        submission.SubmissionText = request.SubmissionText;
        submission.SubmittedOn = DateTime.Now;
        db.AssignmentSubmissions.Update(submission);
        await db.SaveChangesAsync(ct);

        FileResponse[] files = await db.AssignmentFiles.AsNoTracking()
            .Where(f => f.SubmissionId == submission.Id)
            .Select(f => _mapper.Map<FileResponse>(f))
            .ToArrayAsync(ct);

        return Result<SubmissionDetailResponse>.Success(new(
            _sqidsEncoder.Encode(assignment.Id),
            _sqidsEncoder.Encode(userId),
            submission.SubmissionText, files));
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

        await db.Entry(submission)
            .Reference(s => s.Assignment)
            .LoadAsync(ct);

        return new AssignmentFileResponse(
            _sqidsEncoder.Encode(submission.Assignment.ResourceId),
            _mapper.Map<FileResponse>(file));
    }

    public async Task<Result<AssignmentSubmission>> ValidateFileAsync(
        long resourceId,
        IFormFile file,
        CancellationToken ct)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var userId = _currentUserService.UserId;

        var assignment = await db.Assignments
            .Where(a => a.ResourceId == resourceId)
            .FirstOrDefaultAsync(ct);
        if (assignment is null) return Result<AssignmentSubmission>.Failure(ErrorType.NotFound, "Assignment not found");

        var submission = await db.AssignmentSubmissions
            .FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.UserId == userId, ct);
        submission ??= await CreateEmptySubmissionAsync(db, assignment.Id, userId, ct);

        if (assignment.SubmissionType == SubmissionType.Text)
            return Result<AssignmentSubmission>.Failure(ErrorType.Validation, "Assignment does not accept files.");

        if (assignment.AllowedExtensions is not null)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!assignment.AllowedExtensions.Contains(extension))
                return Result<AssignmentSubmission>.Failure(ErrorType.Validation, $"This assignment does not accept extension of type {extension} (allowed: {assignment.AllowedExtensions}).");
        }

        var maxFileSize = ByteSize.FromKiloBytes(assignment.MaxFileSizeKb);
        if (file.Length > maxFileSize.Bytes)
            return Result<AssignmentSubmission>.Failure(ErrorType.Validation, $"File is too large ({file.Length} > {assignment.MaxFileSizeKb} Kb).");

        if (assignment.MaxFileCount is not null)
        {
            int currentFileCount = await db.AssignmentFiles.AsNoTracking().CountAsync(f => f.SubmissionId == submission.Id, ct);
            if (currentFileCount >= assignment.MaxFileCount)
                return Result<AssignmentSubmission>.Failure(ErrorType.Validation, $"File count exceeded (max is {assignment.MaxFileCount}).");
        }

        return Result<AssignmentSubmission>.Success(submission);
    }

    public async Task<bool> RemoveAsync(long resourceId, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var userId = _currentUserService.UserId;

        var submission = await db.AssignmentSubmissions.FirstOrDefaultAsync(s => s.Assignment.ResourceId == resourceId, ct);
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
        var assignment = await db.Assignments.FirstOrDefaultAsync(a => a.Id == resourceId, ct);

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
}
