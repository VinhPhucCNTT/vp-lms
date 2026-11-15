using AutoMapper;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Common;
using Backend.Api.Services.Courses;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Assignments;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Courses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Content;

public interface IAssignmentService
{
    public Task<AssignmentResponse?> GetDtoByIdAsync(long activityId, CancellationToken ct = default);

    public Task<List<StudentAssignmentSummaryResponse>> QueryStudentAsync(CancellationToken ct = default);

    public Task<List<AssignmentResponse>> QueryAsync(CancellationToken ct = default);

    public Task<AssignmentResponse?> CreateAsync(long moduleId, AssignmentRequest request, CancellationToken ct = default);

    public Task<AssignmentResponse?> UpdateAsync(long activityId, AssignmentRequest request);

    public Task<Result<bool>> SetPublishStatusAsync(long activityId, bool isPublished, CancellationToken ct = default);
}

public class AssignmentService(
    IDbContextFactory<AppDbContext> dbFactory,
    IMapper mapper,
    CurrentUserService currentUserService) : IAssignmentService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IMapper _mapper = mapper;
    private readonly CurrentUserService _currentUserService = currentUserService;

    public async Task<AssignmentResponse?> GetDtoByIdAsync(long activityId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var userId = _currentUserService.UserId;
        return await db.Assignments
            .AsNoTracking()
            .Where(a => a.ActivityId == activityId)
            .Where(a => a.Activity.IsPublished)
            .Where(a => db.Enrollments.Any(e =>
                e.CourseId == a.Activity.Module.CourseId &&
                e.UserId == userId))
            .Select(a => new AssignmentResponse(
                _mapper.Map<ActivityDetailResponse>(a.Activity),
                _mapper.Map<AssignmentInfo>(a)
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<StudentAssignmentSummaryResponse>> QueryStudentAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var userId = _currentUserService.UserId;

        var assignments = await db.Assignments
            .AsNoTracking()
            .Where(a => a.Activity.IsPublished)
            .Where(a => db.Enrollments.Any(e =>
                e.CourseId == a.Activity.Module.CourseId &&
                e.UserId == userId))
            .Include(a => a.Activity)
                .ThenInclude(r => r.Module)
                    .ThenInclude(m => m.Course)
            .Include(a => a.Submissions.Where(s => s.UserId == userId))
                .ThenInclude(s => s.Grade)
            .Include(a => a.Submissions.Where(s => s.UserId == userId))
                .ThenInclude(s => s.Files)
            .OrderBy(a => a.Activity.OrderIndex)
            .ToListAsync(ct);

        return assignments.ConvertAll(assignment =>
        {
            var submission = assignment.Submissions.SingleOrDefault();
            var status = GetStudentStatus(assignment, submission);
            var assignmentResponse = new AssignmentResponse(
                _mapper.Map<ActivityDetailResponse>(assignment.Activity),
                _mapper.Map<AssignmentInfo>(assignment));

            return new StudentAssignmentSummaryResponse(
                assignmentResponse,
                _mapper.Map<CourseResponse>(assignment.Activity.Module.Course),
                status,
                submission?.SubmittedOn,
                submission?.Grade?.Score,
                submission?.Grade?.FeedbackText,
                submission?.Files.Count ?? 0);
        });
    }

    public async Task<List<AssignmentResponse>> QueryAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var assignments = await db.Assignments
            .AsNoTracking()
            .Include(x => x.Activity)
            .ToListAsync(ct);

        return assignments.ConvertAll(x => new AssignmentResponse(
            _mapper.Map<ActivityDetailResponse>(x.Activity),
            _mapper.Map<AssignmentInfo>(x)));
    }

    public async Task<AssignmentResponse?> CreateAsync(long moduleId, AssignmentRequest request, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        if (request.Info.OpenDate is not null &&
            request.Info.CloseDate is not null &&
            request.Info.OpenDate > request.Info.CloseDate)
        {

            return null;
        }


        var activity = await ActivityService.CreateActivityAsync(db, moduleId, request.ActivityInfo, ActivityType.Assignment, ct);
        var assignment = new Assignment
        {
            ActivityId = activity.Id,
            InstructionsMD = request.Info.InstructionsMD,
            SubmissionType = request.Info.SubmissionType,
            AllowedExtensions = request.Info.AllowedExtensions,
            MaxFileSizeKb = request.Info.MaxFileSizeKb,
            MaxFileCount = request.Info.MaxFileCount,
            MinTextLength = request.Info.MinTextLength,
            MaxTextLength = request.Info.MaxTextLength,
            OpenDate = request.Info.OpenDate,
            CloseDate = request.Info.CloseDate,
            GradingSchemaJson = request.Info.GradingSchemaJson
        };

        db.Assignments.Add(assignment);
        await db.SaveChangesAsync(ct);
        return new AssignmentResponse(
            _mapper.Map<ActivityDetailResponse>(activity),
            _mapper.Map<AssignmentInfo>(assignment));
    }

    // public async Task<bool> ValidateGradingSchemaAsync(string? GradingSchemaJson) { }

    public async Task<AssignmentResponse?> UpdateAsync(long activityId, AssignmentRequest request)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var assignment = await db.Assignments.FirstOrDefaultAsync(a => a.Id == activityId);
        if (assignment is null)
            return null;

        if (request.Info.OpenDate is not null &&
            request.Info.CloseDate is not null &&
            request.Info.OpenDate > request.Info.CloseDate)
        {

            return null;
        }

        var activity = await ActivityService.UpdateActivityAsync(db, assignment.ActivityId, request.ActivityInfo);
        assignment.ActivityId = activity.Id;
        assignment.InstructionsMD = request.Info.InstructionsMD;
        assignment.SubmissionType = request.Info.SubmissionType;
        assignment.AllowedExtensions = request.Info.AllowedExtensions;
        assignment.MaxFileSizeKb = request.Info.MaxFileSizeKb;
        assignment.MaxFileCount = request.Info.MaxFileCount;
        assignment.MinTextLength = request.Info.MinTextLength;
        assignment.MaxTextLength = request.Info.MaxTextLength;
        assignment.OpenDate = request.Info.OpenDate;
        assignment.CloseDate = request.Info.CloseDate;
        assignment.GradingSchemaJson = request.Info.GradingSchemaJson;

        db.Assignments.Update(assignment);
        await db.SaveChangesAsync();
        return _mapper.Map<AssignmentResponse>(assignment);
    }

    public async Task<Result<bool>> SetPublishStatusAsync(long activityId, bool isPublished, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var activity = await db.CourseActivities.FirstOrDefaultAsync(r => r.Type == ActivityType.Assignment && r.Id == activityId, ct);
        if (activity is null)
            return Result<bool>.Failure(ResultError.NotFound, "Assignment not found.");

        activity.IsPublished = isPublished;
        await db.SaveChangesAsync(ct);

        return Result<bool>.Success(activity.IsPublished);
    }

    private static string GetStudentStatus(
        Assignment assignment,
        AssignmentSubmission? submission)
    {
        if (submission?.Grade is not null)
            return "graded";

        if (submission?.SubmittedOn is not null)
            return "submitted";

        if (assignment.CloseDate is not null && DateTime.UtcNow >= assignment.CloseDate)
            return "overdue";

        return "pending";
    }

    // TODO: Implement
    // public async Task GetAssignmentStatsAsync(long activityId) { }

}
