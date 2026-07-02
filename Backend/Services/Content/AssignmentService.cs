using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Services.Common;
using Backend.Core.Types;
using Backend.Core.Entities.Courses;
using Backend.Core.Entities.Resources;
using Backend.Core.Entities.Submissions;
using AutoMapper;

namespace Backend.Services.Content;

public class AssignmentService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;

    public async Task<AssignmentResponse?> GetAssignmentByIdAsync(long resourceId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Assignments
            .AsNoTracking()
            .Where(a => a.ResourceId == resourceId)
            .Select(a => _mapper.Map<AssignmentResponse>(a))
            .FirstOrDefaultAsync();
    }

    public async Task<AssignmentResponse?> CreateAssignmentAsync(ModuleResource resource, AssignmentRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var assignment = new Assignment
        {
            ResourceId = resource.Id,
            InstructionsMarkdown = request.InstructionsMarkdown,
            AllowedFileTypes = request.AllowedFileTypes,
            MaxFileSizeKb = request.MaxFileSizeKb,
            MaxAttempt = request.MaxAttempt,
            SubmissionType = request.SubmissionType,
            GradingSchemaJson = request.GradingSchemaJson
        };
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();
        return _mapper.Map<AssignmentResponse>(assignment);
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
        assignment.MaxAttempt = request.MaxAttempt;
        assignment.SubmissionType = request.SubmissionType;
        assignment.GradingSchemaJson = request.GradingSchemaJson;

        db.Assignments.Update(assignment);
        await db.SaveChangesAsync();
        return _mapper.Map<AssignmentResponse>(assignment);
    }

    public async Task<List<SubmissionResponse>?> GetSubmissionsAsync(long assignmentId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId)
            .Select(s => _mapper.Map<SubmissionResponse>(s))
            .ToListAsync();
    }

    public async Task<AssignmentGradeResponse?> GetGradeBySubmissionAsync(long submissionId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AssignmentGrades
            .AsNoTracking()
            .Where(g => g.SubmissionId == submissionId)
            .Select(g => _mapper.Map<AssignmentGradeResponse>(g))
            .FirstOrDefaultAsync();
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
            .Select(s => _mapper.Map<SubmissionResponse>(s))
            .ToListAsync();
    }

    public async Task<List<SubmissionResponse>?> GetStudentSubmissionsAsync(long assignmentId, long studentUserId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => s.AssignmentId == assignmentId && s.UserId == studentUserId)
            .Select(s => _mapper.Map<SubmissionResponse>(s))
            .ToListAsync();
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
        return _mapper.Map<SubmissionResponse>(submission);
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
}
