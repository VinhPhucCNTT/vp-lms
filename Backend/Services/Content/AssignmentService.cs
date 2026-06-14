using Backend.Data;
using Backend.Core.Common;
using Microsoft.EntityFrameworkCore;
using Backend.Services.Common;
using Backend.Core.Types;
using Backend.Core.Entities.Courses;
using Sqids;
using Backend.Core.Entities.Resources;
using Backend.Core.Entities.Submissions;

namespace Backend.Services.Content;

public class AssignmentService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    SqidsEncoder<long> sqidsEncoder)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly SqidsEncoder<long> _sqidsEncoder = sqidsEncoder;

    public async Task<AssignmentResponse?> GetAssignmentByIdAsync(long resourceId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Assignments
            .AsNoTracking()
            .Where(a => a.ResourceId == resourceId)
            .Select(a => new AssignmentResponse(
                _sqidsEncoder.Encode(a.Id),
                a.InstructionsMarkdown,
                a.AllowedFileTypes,
                a.MaxFileSizeKb,
                a.SubmissionType,
                a.GradingSchemaJson
            )).FirstOrDefaultAsync();
    }

    public async Task<AssignmentResponse> CreateAssignmentAsync(ModuleResource resource, AssignmentRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var assignment = new Assignment
        {
            ResourceId = resource.Id,
            InstructionsMarkdown = request.InstructionsMarkdown,
            AllowedFileTypes = request.AllowedFileTypes,
            MaxFileSizeKb = request.MaxFileSizeKb,
            SubmissionType = request.SubmissionType,
            GradingSchemaJson = request.GradingSchemaJson
        };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();
        return new AssignmentResponse(
            _sqidsEncoder.Encode(assignment.Id),
            assignment.InstructionsMarkdown,
            assignment.AllowedFileTypes,
            assignment.MaxFileSizeKb,
            assignment.SubmissionType,
            assignment.GradingSchemaJson
        );
    }

    // public async Task<bool> ValidateGradingSchemaAsync(string? GradingSchemaJson) { }

    // public async Task ValidateGradingSchemaAsync(string? GradingSchemaJson) { }

    // public async Task ValidateGradingSchemaAsync(string? GradingSchemaJson) { }

    public async Task<AssignmentResponse?> UpdateAssignmentAsync(long assignmentId, AssignmentRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var assignment = await db.Assignments.FirstOrDefaultAsync(a => a.Id == assignmentId);
        if (assignment is null)
            return null;

        assignment.InstructionsMarkdown = request.InstructionsMarkdown;
        assignment.AllowedFileTypes = request.AllowedFileTypes;
        assignment.MaxFileSizeKb = request.MaxFileSizeKb;
        assignment.SubmissionType = request.SubmissionType;
        assignment.GradingSchemaJson = request.GradingSchemaJson;

        db.Assignments.Update(assignment);
        await db.SaveChangesAsync();
        return new AssignmentResponse(
            _sqidsEncoder.Encode(assignment.Id),
            assignment.InstructionsMarkdown,
            assignment.AllowedFileTypes,
            assignment.MaxFileSizeKb,
            assignment.SubmissionType,
            assignment.GradingSchemaJson
        );
    }

    public async Task<List<SubmissionResponse>?> GetSubmissionsAsync(long assignmentId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId)
            .Select(s => new SubmissionResponse(
                _sqidsEncoder.Encode(s.AssignmentId),
                _sqidsEncoder.Encode(s.UserId),
                s.SubmissionText,
                s.FileUrl,
                s.FileName
            )).ToListAsync();
    }

    public async Task<AssignmentGradeResponse?> GetGradeBySubmissionAsync(long submissionId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AssignmentGrades
            .AsNoTracking()
            .Where(g => g.SubmissionId == submissionId)
            .Select(g => new AssignmentGradeResponse(
                _sqidsEncoder.Encode(g.SubmissionId),
                _sqidsEncoder.Encode(g.GraderId),
                g.Score,
                g.FeedbackText
            )).FirstOrDefaultAsync();
    }

    public async Task<List<AssignmentGradeResponse>?> GetGradesByAssignmentAsync(long assignmentId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await GetGradesAsync(db, g => g.Submission.AssignmentId == assignmentId);
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

    public async Task<List<AssignmentGradeResponse>?> GetStudentGradesAsync(long assignmentId, long studentUserId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await GetGradesAsync(db, g => g.Submission.AssignmentId == assignmentId && g.Submission.UserId == studentUserId);
    }

    public async Task<List<SubmissionResponse>?> GetUngradedSubmissionsAsync(long assignmentId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId && s.Grade == null)
            .Select(s => new SubmissionResponse(
                _sqidsEncoder.Encode(s.AssignmentId),
                _sqidsEncoder.Encode(s.UserId),
                s.SubmissionText,
                s.FileUrl,
                s.FileName
            )).ToListAsync();
    }

    public async Task<List<SubmissionResponse>?> GetStudentSubmissionsAsync(long assignmentId, long studentUserId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId && s.UserId == studentUserId)
            .Select(s => new SubmissionResponse(
                _sqidsEncoder.Encode(s.AssignmentId),
                _sqidsEncoder.Encode(s.UserId),
                s.SubmissionText,
                s.FileUrl,
                s.FileName
            )).ToListAsync();
    }

    // TODO: Implement
    // public async Task GetAssignmentStatsAsync(long assignmentId) { }

    public async Task<SubmissionResponse?> SubmitAssignmentAsync(long assignmentId, SubmissionRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var assignment = await db.Assignments.FirstOrDefaultAsync(a => a.Id == assignmentId);
        if (assignment is null)
            return null;

        var currentUserId = _currentUserService.UserId;
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
        return new SubmissionResponse(
            _sqidsEncoder.Encode(submission.AssignmentId),
            _sqidsEncoder.Encode(currentUserId),
            submission.SubmissionText,
            submission.FileUrl,
            submission.FileName
        );
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
        return new AssignmentGradeResponse(
            _sqidsEncoder.Encode(grade.SubmissionId),
            _sqidsEncoder.Encode(currentUserId),
            grade.Score,
            grade.FeedbackText
        );
    }

    private async Task<List<AssignmentGradeResponse>?> GetGradesAsync(
        AppDbContext db,
        System.Linq.Expressions.Expression<Func<AssignmentGrade, bool>> predicate)
    {
        return await db.AssignmentGrades
            .AsNoTracking()
            .Where(predicate)
            .Select(g => new AssignmentGradeResponse(
                _sqidsEncoder.Encode(g.Submission.AssignmentId),
                _sqidsEncoder.Encode(g.GraderId),
                g.Score,
                g.FeedbackText
            )).ToListAsync();
    }
}
