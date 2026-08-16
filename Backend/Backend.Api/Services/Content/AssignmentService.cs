using Backend.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Content;
using AutoMapper;
using Backend.Api.Services.Courses;
using Backend.Api.Core.Common;
using Backend.Api.Services.Common;

namespace Backend.Api.Services.Content;

public class AssignmentService(
    IDbContextFactory<AppDbContext> dbFactory,
    IMapper mapper,
    CurrentUserService currentUserService)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IMapper _mapper = mapper;
    private readonly CurrentUserService _currentUserService = currentUserService;

    public async Task<AssignmentResponse?> GetDtoByIdAsync(long resourceId, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var userId = _currentUserService.UserId;
        return await db.Assignments
            .AsNoTracking()
            .Where(a => a.ResourceId == resourceId)
            .Where(a => a.Resource.IsPublished)
            .Where(a => db.Enrollments.Any(e =>
                e.CourseId == a.Resource.Module.CourseId &&
                e.UserId == userId))
            .Select(a => new AssignmentResponse(
                _mapper.Map<ResourceDetailResponse>(a.Resource),
                _mapper.Map<AssignmentInfo>(a)
            ))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<List<StudentAssignmentSummaryResponse>> QueryStudentAsync(CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var userId = _currentUserService.UserId;

        var assignments = await db.Assignments
            .AsNoTracking()
            .Where(a => a.Resource.IsPublished)
            .Where(a => db.Enrollments.Any(e =>
                e.CourseId == a.Resource.Module.CourseId &&
                e.UserId == userId))
            .Include(a => a.Resource)
                .ThenInclude(r => r.Module)
                    .ThenInclude(m => m.Course)
            .Include(a => a.Submissions.Where(s => s.UserId == userId))
                .ThenInclude(s => s.Grade)
            .Include(a => a.Submissions.Where(s => s.UserId == userId))
                .ThenInclude(s => s.Files)
            .OrderBy(a => a.Resource.Module.Course.Code)
            .ThenBy(a => a.Resource.OrderIndex)
            .ToListAsync(ct);

        return assignments.Select(assignment =>
        {
            var submission = assignment.Submissions.SingleOrDefault();
            var status = GetStudentStatus(assignment, submission);
            var assignmentResponse = new AssignmentResponse(
                _mapper.Map<ResourceDetailResponse>(assignment.Resource),
                _mapper.Map<AssignmentInfo>(assignment));

            return new StudentAssignmentSummaryResponse(
                assignmentResponse,
                _mapper.Map<CourseResponse>(assignment.Resource.Module.Course),
                status,
                submission?.SubmittedOn,
                submission?.Grade?.Score,
                submission?.Grade?.FeedbackText,
                submission?.Files.Count ?? 0);
        }).ToList();
    }

    public async Task<List<AssignmentResponse>> QueryAsync(CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var assignments = await db.Assignments
            .AsNoTracking()
            .Include(x => x.Resource)
            .ToListAsync(ct);

        return assignments.Select(x => new AssignmentResponse(
            _mapper.Map<ResourceDetailResponse>(x.Resource),
            _mapper.Map<AssignmentInfo>(x))).ToList();
    }

    public async Task<AssignmentResponse?> CreateAsync(long moduleId, AssignmentRequest request, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);

        if (request.Info.OpenDate is not null &&
            request.Info.CloseDate is not null &&
            request.Info.OpenDate > request.Info.CloseDate)
            return null;

        var resource = await ResourceService.CreateResourceAsync(db, moduleId, request.ResourceInfo, ResourceType.Assignment, ct);
        var assignment = new Assignment
        {
            ResourceId = resource.Id,
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
            _mapper.Map<ResourceDetailResponse>(resource),
            _mapper.Map<AssignmentInfo>(assignment));
    }

    // public async Task<bool> ValidateGradingSchemaAsync(string? GradingSchemaJson) { }

    public async Task<AssignmentResponse?> UpdateAsync(long resourceId, AssignmentRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var assignment = await db.Assignments.FirstOrDefaultAsync(a => a.Id == resourceId);
        if (assignment is null)
            return null;

        if (request.Info.OpenDate is not null &&
            request.Info.CloseDate is not null &&
            request.Info.OpenDate > request.Info.CloseDate)
            return null;

        var resource = await ResourceService.UpdateResourceAsync(db, assignment.ResourceId, request.ResourceInfo);
        assignment.ResourceId = resource.Id;
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

    public async Task<Result<bool>> SetPublishStatusAsync(long resourceId, bool isPublished, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var resource = await db.CourseResources.FirstOrDefaultAsync(r => r.Type == ResourceType.Assignment && r.Id == resourceId, ct);
        if (resource is null)
            return Result<bool>.Failure(ErrorType.NotFound, "Assignment not found.");

        resource.IsPublished = isPublished;
        await db.SaveChangesAsync(ct);

        return Result<bool>.Success(resource.IsPublished);
    }

    private static string GetStudentStatus(
        Assignment assignment,
        Backend.Persistence.Entities.Assignments.AssignmentSubmission? submission)
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
    // public async Task GetAssignmentStatsAsync(long resourceId) { }

}
