using AutoMapper;
using Backend.Api.Core.Authorization;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Common;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Learning;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Courses;

public interface IActivityService
{
    public Task<ActivityDetailResponse?> GetActivityByIdAsync(long activityId, CancellationToken ct = default);

    public Task<PaginatedResponse<ActivityResponse>?> GetPublishedActivitiesAsync(long moduleId, PageRequest page, CancellationToken ct = default);

    public Task<PaginatedResponse<ActivityResponse>> GetUnpublishedActivitiesAsync(long moduleId, PageRequest page, CancellationToken ct = default);

    public Task<ActivityProgressResponse?> GetActivityProgressAsync(long activityId, CancellationToken ct = default);

    public Task<ActivityProgressResponse?> MarkActivityCompletedAsync(long activityId, CancellationToken ct = default);

    public Task<bool> DeleteActivityAsync(long activityId, CancellationToken ct = default);

    public Task<bool> ReorderActivityAsync(long activityId, int orderIndex, CancellationToken ct = default);

    public Task<bool> CheckOwnerAsync(long activityId, CancellationToken ct = default);
}

public class ActivityService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper,
    ICourseAuthorization courseAuthorization) : IActivityService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;

    public async Task<ActivityDetailResponse?> GetActivityByIdAsync(long activityId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var currentUserId = _currentUserService.UserId;
        return await db.CourseActivities
            .AsNoTracking()
            .Where(r => r.Id == activityId)
            .Where(r => r.IsPublished || r.Module.Course.InstructorId == currentUserId)
            .Select(r => _mapper.Map<ActivityDetailResponse>(r))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PaginatedResponse<ActivityResponse>?> GetPublishedActivitiesAsync(long moduleId, PageRequest page, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var module = await db.CourseModules.AsNoTracking()
            .Where(m => m.Id == moduleId && !m.IsDeleted && !m.Course.IsDeleted)
            .Select(m => new { m.CourseId, m.IsPublished })
            .FirstOrDefaultAsync(ct);
        if (module is null)
            return null;

        var access = await courseAuthorization.GetReadAccessAsync(module.CourseId, ct);
        if (access is null || (!access.IsOwner && !module.IsPublished))
            return null;

        var activities = db.CourseActivities
            .AsNoTracking()
            .Where(r => r.ModuleId == moduleId && !r.IsDeleted && !r.Module.IsDeleted && !r.Module.Course.IsDeleted)
            .Where(r => access.IsOwner || r.IsPublished);

        var list = await activities
            .OrderBy(r => r.OrderIndex).ThenBy(r => r.Id)
            .Skip((page.PageNumber - 1) * page.PageSize)
            .Take(page.PageSize)
            .Select(r => _mapper.Map<ActivityResponse>(r))
            .ToListAsync(ct);

        return new PaginatedResponse<ActivityResponse>(
            page.PageNumber,
            page.PageSize,
            await activities.CountAsync(ct),
            list);
    }

    public async Task<PaginatedResponse<ActivityResponse>> GetUnpublishedActivitiesAsync(long moduleId, PageRequest page, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var currentUserId = _currentUserService.UserId;

        var activities = db.CourseActivities
            .AsNoTracking()
            .Where(r => r.ModuleId == moduleId && !r.IsPublished && r.Module.Course.InstructorId == currentUserId);

        var list = await activities
            .Skip((page.PageNumber - 1) * page.PageSize)
            .Take(page.PageSize)
            .Select(r => _mapper.Map<ActivityResponse>(r))
            .ToListAsync(ct);

        return new PaginatedResponse<ActivityResponse>(
            page.PageNumber,
            page.PageSize,
            await activities.CountAsync(ct),
            list);
    }

    public async Task<ActivityProgressResponse?> GetActivityProgressAsync(long activityId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var userId = _currentUserService.UserId;

        var activityExists = await db.CourseActivities
            .AsNoTracking()
            .AnyAsync(r => r.Id == activityId && r.IsPublished, ct);
        if (!activityExists)
            return null;

        var progress = await db.ActivityProgress
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ActivityId == activityId && p.UserId == userId, ct);

        return progress is null
            ? new ActivityProgressResponse(false, null, null)
            : new ActivityProgressResponse(
                progress.IsCompleted,
                progress.CompletedAt,
                progress.LastAccessedAt);
    }

    public async Task<ActivityProgressResponse?> MarkActivityCompletedAsync(long activityId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var userId = _currentUserService.UserId;

        var activityExists = await db.CourseActivities
            .AsNoTracking()
            .AnyAsync(r => r.Id == activityId && r.IsPublished, ct);
        if (!activityExists)
            return null;

        var progress = await db.ActivityProgress
            .FirstOrDefaultAsync(p => p.ActivityId == activityId && p.UserId == userId, ct);
        var now = DateTime.UtcNow;

        if (progress is null)
        {
            progress = new ActivityProgress
            {
                UserId = userId,
                ActivityId = activityId,
                IsCompleted = true,
                CompletedAt = now,
                LastAccessedAt = now
            };
            db.ActivityProgress.Add(progress);
        }
        else
        {
            progress.IsCompleted = true;
            progress.CompletedAt ??= now;
            progress.LastAccessedAt = now;
        }

        await db.SaveChangesAsync(ct);

        return new ActivityProgressResponse(
            progress.IsCompleted,
            progress.CompletedAt,
            progress.LastAccessedAt);
    }

    public async Task<bool> DeleteActivityAsync(long activityId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var activity = await db.CourseActivities.FirstOrDefaultAsync(r => r.Id == activityId, ct);
        if (activity is null)
            return false;
        db.CourseActivities.Remove(activity);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ReorderActivityAsync(long activityId, int orderIndex, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            var target = await db.CourseActivities.FirstOrDefaultAsync(m => m.Id == activityId, ct);
            if (target == null) return false;

            var activities = db.CourseActivities
                .Where(m => m.ModuleId == target.ModuleId);
            var count = await activities.CountAsync(ct);

            if (orderIndex < 0) orderIndex = 0;
            orderIndex = Math.Min(orderIndex, count + 1);

            await activities
                .Where(m => m.OrderIndex >= orderIndex)
                .ExecuteUpdateAsync(m => m.SetProperty(m => m.OrderIndex, m => m.OrderIndex + 1), ct);
            target.OrderIndex = orderIndex;
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(ct);
            return false;
        }

        return true;
    }

    public async Task<bool> CheckOwnerAsync(long activityId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var currentUserId = _currentUserService.UserId;
        return await db.CourseActivities
            .AsNoTracking()
            .Where(c => c.Id == activityId && c.Module.Course.InstructorId == currentUserId)
            .AnyAsync(ct);
    }

    static public async Task<List<long>> GetCourseActivityIdsAsync(AppDbContext db, long courseId, CancellationToken ct = default)
    {
        var moduleIds = await db.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId)
            .Select(m => m.Id)
            .ToListAsync(ct);
        return await db.CourseActivities
            .AsNoTracking()
            .Where(r => moduleIds.Contains(r.ModuleId))
            .Select(r => r.Id)
            .ToListAsync(ct);
    }

    static public async Task<CourseProgress> GetCourseProgressAsync(AppDbContext db, long courseId, long userId, CancellationToken ct = default)
    {
        var activityIds = await GetCourseActivityIdsAsync(db, courseId, ct);
        var completed = await db.ActivityProgress
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.IsCompleted && activityIds.Contains(r.ActivityId))
            .CountAsync(ct);
        return new CourseProgress(Completed: completed, Total: activityIds.Count);
    }

    static public async Task<CourseActivity> CreateActivityAsync(AppDbContext db, long moduleId, ActivityRequestInfo info, ActivityType type, CancellationToken ct = default)
    {
        var activity = new CourseActivity
        {
            ModuleId = moduleId,
            Type = type,
            Title = info.Title,
            OrderIndex = info.OrderIndex,
            IsPublished = info.IsPublished,
        };

        db.CourseActivities.Add(activity);
        await db.SaveChangesAsync(ct);
        return activity;
    }

    static public async Task<CourseActivity> UpdateActivityAsync(AppDbContext db, long activityId, ActivityRequestInfo info, CancellationToken ct = default)
    {
        var activity = await db.CourseActivities.FirstOrDefaultAsync(r => r.Id == activityId, ct);
        if (activity is not null)
        {
            activity.Title = info.Title;
            activity.OrderIndex = info.OrderIndex;
            activity.IsPublished = info.IsPublished;

            db.CourseActivities.Update(activity);
            await db.SaveChangesAsync(ct);

            return activity;
        }

        throw new Exception($"UpdateActivityAsync: Activity id {activityId} does not exist.");
    }
}
