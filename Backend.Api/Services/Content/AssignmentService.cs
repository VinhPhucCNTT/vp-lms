using Backend.Api.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Core.Types;
using Backend.Api.Core.Entities.Courses;
using Backend.Api.Core.Entities.Content;
using AutoMapper;
using Backend.Api.Services.Courses;
using Backend.Api.Core.Common;

namespace Backend.Api.Services.Content;

public class AssignmentService(
    IDbContextFactory<AppDbContext> dbFactory,
    IMapper mapper)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IMapper _mapper = mapper;

    public async Task<AssignmentResponse?> GetDtoByIdAsync(long resourceId)
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

    // TODO: Implement
    // public async Task GetAssignmentStatsAsync(long resourceId) { }

}
