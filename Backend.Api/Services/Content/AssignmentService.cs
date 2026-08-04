using Backend.Api.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Services.Common;
using Backend.Api.Core.Types;
using Backend.Api.Core.Entities.Courses;
using Backend.Api.Core.Entities.Content;
using AutoMapper;
using Backend.Api.Services.Courses;
using Backend.Api.Core.Entities.Assignments;
using System.ComponentModel.DataAnnotations;
using Backend.Api.Core.Common;
using ByteSizeLib;

namespace Backend.Api.Services.Content;

public class AssignmentService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    FileService fileService,
    IMapper mapper)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly FileService _fileService = fileService;
    private readonly IMapper _mapper = mapper;

    public async Task<AssignmentResponse?> GetAssignmentByIdAsync(long resourceId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Assignments
            .AsNoTracking()
            .Where(a => a.ResourceId == resourceId)
            .Select(a => new AssignmentResponse(
                _mapper.Map<ResourceDetailResponse>(a.Resource),
                _mapper.Map<AssignmentInfo>(a)
            ))
            .FirstOrDefaultAsync();
    }

    public async Task<List<SubmissionResponse>?> GetSubmissionsAsync(long resourceId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == resourceId)
            .Select(s => _mapper.Map<SubmissionResponse>(s))
            .ToListAsync();
    }

    public async Task<SubmissionResponse?> GetSubmissionByUserIdAsync(long resourceId, long userId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == resourceId && s.UserId == userId)
            .Select(s => _mapper.Map<SubmissionResponse>(s))
            .FirstOrDefaultAsync();
    }

    public async Task<AssignmentGradeResponse?> GetSubmissionGradeAsync(long submissionId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AssignmentGrades
            .AsNoTracking()
            .Where(g => g.SubmissionId == submissionId)
            .Select(g => _mapper.Map<AssignmentGradeResponse>(g))
            .FirstOrDefaultAsync();
    }

    public async Task<List<AssignmentGradeResponse>?> GetAssignmentGradesAsync(long resourceId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await GetGradesAsync(db, g => g.Submission.AssignmentId == resourceId);
    }

    public async Task<List<AssignmentGradeResponse>?> GetGradedBySelfAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await GetGradesAsync(db, g => g.GraderId == currentUserId);
    }

    public async Task<List<AssignmentGradeResponse>?> GetGradesOfSelfAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await GetGradesAsync(db, g => g.Submission.UserId == currentUserId);
    }

    public async Task<List<AssignmentGradeResponse>?> GetStudentGradesAsync(long resourceId, long studentUserId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await GetGradesAsync(db, g => g.Submission.AssignmentId == resourceId && g.Submission.UserId == studentUserId);
    }

    public async Task<List<SubmissionResponse>?> GetUngradedSubmissionsAsync(long resourceId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == resourceId && s.Grade == null)
            .Select(s => _mapper.Map<SubmissionResponse>(s))
            .ToListAsync();
    }

    public async Task<AssignmentResponse?> CreateAssignmentAsync(AssignmentRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var resource = await ResourceService.CreateResourceAsync(db, request.ResourceInfo, ResourceType.Assignment);
        var assignment = new Assignment
        {
            ResourceId = resource.Id,
            InstructionsMD = request.Info.InstructionsMD,
            AllowedFileTypes = request.Info.AllowedFileTypes,
            MaxFileSizeKb = request.Info.MaxFileSizeKb,
            SubmissionType = request.Info.SubmissionType,
            GradingSchemaJson = request.Info.GradingSchemaJson
        };

        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();
        return new AssignmentResponse(
            _mapper.Map<ResourceDetailResponse>(resource),
            _mapper.Map<AssignmentInfo>(assignment));
    }

    // public async Task<bool> ValidateGradingSchemaAsync(string? GradingSchemaJson) { }

    public async Task<AssignmentResponse?> UpdateAssignmentAsync(long resourceId, AssignmentRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var assignment = await db.Assignments.FirstOrDefaultAsync(a => a.Id == resourceId);
        if (assignment is null)
            return null;

        var resource = await ResourceService.UpdateResourceAsync(db, assignment.ResourceId, request.ResourceInfo);

        assignment.InstructionsMD = request.Info.InstructionsMD;
        assignment.AllowedFileTypes = request.Info.AllowedFileTypes;
        assignment.MaxFileSizeKb = request.Info.MaxFileSizeKb;
        assignment.SubmissionType = request.Info.SubmissionType;
        assignment.GradingSchemaJson = request.Info.GradingSchemaJson;

        db.Assignments.Update(assignment);
        await db.SaveChangesAsync();
        return _mapper.Map<AssignmentResponse>(assignment);
    }

    // TODO: Implement
    // public async Task GetAssignmentStatsAsync(long resourceId) { }

    public async Task<SubmissionResponse?> SubmitAssignmentAsync(long resourceId, SubmissionRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var assignment = await db.Assignments.FirstOrDefaultAsync(a => a.ResourceId == resourceId);
        var currentUserId = _currentUserService.UserId;
        if (assignment is null)
            return null;

        var existingSubmission = await db.AssignmentSubmissions.FirstOrDefaultAsync(s => s.AssignmentId == assignment.Id && s.UserId == currentUserId);
        if (existingSubmission is null)
        {
            var submission = new AssignmentSubmission
            {
                AssignmentId = assignment.Id,
                UserId = currentUserId,
                SubmissionText = request.SubmissionText,
                FileUrl = request.FileUrl,
                FileName = request.FileName
            };

            db.AssignmentSubmissions.Add(submission);
            await db.SaveChangesAsync();
            return _mapper.Map<SubmissionResponse>(submission);
        }
        else
        {
            existingSubmission.SubmissionText = request.SubmissionText;
            existingSubmission.FileUrl = request.FileUrl;
            existingSubmission.FileName = request.FileName;

            db.AssignmentSubmissions.Update(existingSubmission);
            await db.SaveChangesAsync();
            return _mapper.Map<SubmissionResponse>(existingSubmission);
        }
    }

    public async Task<AssignmentFileResponse> UploadAssignmentFileAsync(Assignment assignment, IFormFile file, CancellationToken ct)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        await using var stream = file.OpenReadStream();
        var fileId = await _fileService.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            _currentUserService.UserId,
            FileCategory.AssignmentSubmission,
            ct);

        int fileCount = await db.AssignmentFiles.AsNoTracking().CountAsync(f => f.AssignmentId == assignment.Id, ct);
        var assignmentFile = new AssignmentFile
        {
            AssignmentId = assignment.Id,
            FileId = fileId,
            OrderIndex = fileCount
        };
        db.AssignmentFiles.Add(assignmentFile);
        await db.SaveChangesAsync(ct);

        return new AssignmentFileResponse(assignment.ResourceId, fileId);
    }

    public async Task<AssignmentGradeResponse?> GradeSubmissionAsync(long submissionId, AssignmentGradeRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var submission = await db.AssignmentSubmissions.FirstOrDefaultAsync(s => s.Id == submissionId);
        if (submission is null)
            return null;

        var currentUserId = _currentUserService.UserId;
        var grade = new AssignmentGrade
        {
            SubmissionId = submission.Id,
            GraderId = currentUserId,
            Score = request.Score,
            FeedbackText = request.FeedbackText
        };

        db.AssignmentGrades.Add(grade);
        await db.SaveChangesAsync();
        return _mapper.Map<AssignmentGradeResponse>(grade);
    }

    public async Task<Result<Assignment>> ValidateSubmittedFileAsync(
        long resourceId,
        IFormFile file,
        CancellationToken ct)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var assignment = await db.Assignments
            .Where(a => a.ResourceId == resourceId)
            .FirstOrDefaultAsync(ct);
        if (assignment is null) return Result<Assignment>.Failure(ErrorType.NotFound, "Assignment not found");

        if (assignment.SubmissionType == SubmissionType.Text)
            return Result<Assignment>.Failure(ErrorType.Validation, "Assignment does not accept files.");

        if (assignment.AllowedFileTypes is not null)
        {
            var allowedFileTypes = GetAllowedFileTypes(assignment.AllowedFileTypes);
            if (!allowedFileTypes.Contains(file.ContentType))
                return Result<Assignment>.Failure(ErrorType.Validation, $"This assignment does not accept {file.ContentType}.");
        }

        var maxFileSize = ByteSize.FromKiloBytes(assignment.MaxFileSizeKb);
        if (file.Length > maxFileSize.Bytes)
            return Result<Assignment>.Failure(ErrorType.Validation, $"File is too large ({file.Length} > {assignment.MaxFileSizeKb} Kb).");

        int currentFileCount = await db.AssignmentFiles.AsNoTracking().CountAsync(f => f.AssignmentId == assignment.Id, ct);
        if (currentFileCount >= assignment.MaxFileCount)
            return Result<Assignment>.Failure(ErrorType.Validation, $"File count exceeded (max is {assignment.MaxFileCount}).");

        return Result<Assignment>.Success(assignment);
    }

    private async Task<List<AssignmentGradeResponse>?> GetGradesAsync(
        AppDbContext db,
        System.Linq.Expressions.Expression<Func<AssignmentGrade, bool>> predicate)
    {
        return await db.AssignmentGrades
            .AsNoTracking()
            .Where(predicate)
            .Select(g => _mapper.Map<AssignmentGradeResponse>(g))
            .ToListAsync();
    }

    private static string[] GetAllowedFileTypes(string allowedFileTypes)
    {
        return allowedFileTypes.Split(",");
    }
}
