using AutoMapper;
using Backend.Api.Core.Types;
using Backend.Api.Services.Common;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Learning;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Courses;

public interface IActivityService
{
    public Task<ActivityDetailResponse?> GetActivityByIdAsync(long activityId);

    public Task<List<ActivityResponse>> GetPublishedActivitiesAsync(long moduleId);

    public Task<List<ActivityResponse>> GetUnpublishedActivitiesAsync(long moduleId);

    public Task<ActivityProgressResponse?> GetActivityProgressAsync(long activityId);

    public Task<ActivityProgressResponse?> MarkActivityCompletedAsync(long activityId);

    public Task<bool> DeleteActivityAsync(long activityId);

    public Task<bool> ReorderActivityAsync(long activityId, int orderIndex);

    public Task<bool> CheckOwnerAsync(long activityId);
}

public class ActivityService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper) : IActivityService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;

    public async Task<ActivityDetailResponse?> GetActivityByIdAsync(long activityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.CourseActivities
            .AsNoTracking()
            .Where(r => r.Id == activityId)
            .Where(r => r.IsPublished || r.Module.Course.InstructorId == currentUserId)
            .Select(r => _mapper.Map<ActivityDetailResponse>(r))
            .FirstOrDefaultAsync();
    }

    public async Task<List<ActivityResponse>> GetPublishedActivitiesAsync(long moduleId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        return await db.CourseActivities
            .AsNoTracking()
            .Where(r => r.ModuleId == moduleId && r.IsPublished)
            .Select(r => _mapper.Map<ActivityResponse>(r))
            .ToListAsync();
    }

    public async Task<List<ActivityResponse>> GetUnpublishedActivitiesAsync(long moduleId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.CourseActivities
            .AsNoTracking()
            .Where(r => r.ModuleId == moduleId && !r.IsPublished && r.Module.Course.InstructorId == currentUserId)
            .Select(r => _mapper.Map<ActivityResponse>(r))
            .ToListAsync();
    }

    public async Task<ActivityProgressResponse?> GetActivityProgressAsync(long activityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var userId = _currentUserService.UserId;

        var activityExists = await db.CourseActivities
            .AsNoTracking()
            .AnyAsync(r => r.Id == activityId && r.IsPublished);
        if (!activityExists)
            return null;

        var progress = await db.ActivityProgress
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ActivityId == activityId && p.UserId == userId);

        return progress is null
            ? new ActivityProgressResponse(false, null, null)
            : new ActivityProgressResponse(
                progress.IsCompleted,
                progress.CompletedAt,
                progress.LastAccessedAt);
    }

    public async Task<ActivityProgressResponse?> MarkActivityCompletedAsync(long activityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var userId = _currentUserService.UserId;

        var activityExists = await db.CourseActivities
            .AsNoTracking()
            .AnyAsync(r => r.Id == activityId && r.IsPublished);
        if (!activityExists)
            return null;

        var progress = await db.ActivityProgress
            .FirstOrDefaultAsync(p => p.ActivityId == activityId && p.UserId == userId);
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

        await db.SaveChangesAsync();

        return new ActivityProgressResponse(
            progress.IsCompleted,
            progress.CompletedAt,
            progress.LastAccessedAt);
    }

    public async Task<bool> DeleteActivityAsync(long activityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var activity = await db.CourseActivities.FirstOrDefaultAsync(r => r.Id == activityId);
        if (activity is null)
            return false;
        db.CourseActivities.Remove(activity);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReorderActivityAsync(long activityId, int orderIndex)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var target = await db.CourseActivities.FirstOrDefaultAsync(m => m.Id == activityId);
            if (target == null) return false;

            var activities = db.CourseActivities
                .Where(m => m.ModuleId == target.ModuleId);
            var count = await activities.CountAsync();

            if (orderIndex < 0) orderIndex = 0;
            orderIndex = Math.Min(orderIndex, count + 1);

            await activities
                .Where(m => m.OrderIndex >= orderIndex)
                .ExecuteUpdateAsync(m => m.SetProperty(m => m.OrderIndex, m => m.OrderIndex + 1));
            target.OrderIndex = orderIndex;
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return false;
        }

        return true;
    }

    public async Task<bool> CheckOwnerAsync(long activityId)
    {
        await using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.CourseActivities
            .AsNoTracking()
            .Where(c => c.Id == activityId && c.Module.Course.InstructorId == currentUserId)
            .AnyAsync();
    }

    static public async Task<List<long>> GetCourseActivityIdsAsync(AppDbContext db, long courseId)
    {
        var moduleIds = await db.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId)
            .Select(m => m.Id)
            .ToListAsync();
        return await db.CourseActivities
            .AsNoTracking()
            .Where(r => moduleIds.Contains(r.ModuleId))
            .Select(r => r.Id)
            .ToListAsync();
    }

    static public async Task<CourseProgress> GetCourseProgressAsync(AppDbContext db, long courseId, long userId)
    {
        var activityIds = await GetCourseActivityIdsAsync(db, courseId);
        var completed = await db.ActivityProgress
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.IsCompleted && activityIds.Contains(r.ActivityId))
            .CountAsync();
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

    static public async Task<CourseActivity> UpdateActivityAsync(AppDbContext db, long activityId, ActivityRequestInfo info)
    {
        var activity = await db.CourseActivities.FirstOrDefaultAsync(r => r.Id == activityId);
        if (activity is not null)
        {
            activity.Title = info.Title;
            activity.OrderIndex = info.OrderIndex;
            activity.IsPublished = info.IsPublished;

            db.CourseActivities.Update(activity);
            await db.SaveChangesAsync();

            return activity;
        }

        throw new Exception($"UpdateActivityAsync: Activity id {activityId} does not exist.");
    }
}
