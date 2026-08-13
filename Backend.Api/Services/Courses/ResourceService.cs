using AutoMapper;
using Backend.Api.Core.Entities.Courses;
using Backend.Api.Core.Types;
using Backend.Api.Data;
using Backend.Api.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Courses;

public class ResourceService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper
)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;

    public async Task<ResourceDetailResponse?> GetResourceByIdAsync(long resourceId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.CourseResources
            .AsNoTracking()
            .Where(r => r.Id == resourceId)
            .Where(r => r.IsPublished || r.Module.Course.CreatorId == currentUserId)
            .Select(r => _mapper.Map<ResourceDetailResponse>(r))
            .FirstOrDefaultAsync();
    }

    public async Task<List<ResourceResponse>> GetPublishedResourcesAsync(long moduleId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.CourseResources
            .AsNoTracking()
            .Where(r => r.ModuleId == moduleId && r.IsPublished)
            .Select(r => _mapper.Map<ResourceResponse>(r))
            .ToListAsync();
    }

    public async Task<List<ResourceResponse>> GetUnpublishedResourcesAsync(long moduleId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.CourseResources
            .AsNoTracking()
            .Where(r => r.ModuleId == moduleId && !r.IsPublished && r.Module.Course.CreatorId == currentUserId)
            .Select(r => _mapper.Map<ResourceResponse>(r))
            .ToListAsync();
    }

    public async Task<bool> DeleteResourceAsync(long resourceId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var resource = await db.CourseResources.FirstOrDefaultAsync(r => r.Id == resourceId);
        if (resource is null)
            return false;
        db.CourseResources.Remove(resource);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReorderResourceAsync(long resourceId, int orderIndex)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var target = await db.CourseResources.FirstOrDefaultAsync(m => m.Id == resourceId);
            if (target == null) return false;

            var resources = db.CourseResources
                .Where(m => m.ModuleId == target.ModuleId);
            var count = await resources.CountAsync();

            if (orderIndex < 0) orderIndex = 0;
            orderIndex = Math.Min(orderIndex, count + 1);

            await resources
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

    // public static async Task<bool> SetResourcePublishStatusAsync(AppDbContext db, long resourceId, bool isPublished, CancellationToken ct = default)
    // {
    //     var count = await db.CourseResources
    //         .Where(r => r.Id == resourceId)
    //         .ExecuteUpdateAsync(r => r.SetProperty(r => r.IsPublished, isPublished), ct);
    //
    //     return count > 0;
    // }

    public async Task<int> SetResourcesPublishStatusAsync(long moduleId, List<long> resourceIds, bool isPublished)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.CourseResources
            .Where(r => r.ModuleId == moduleId && resourceIds.Contains(r.Id) && r.Module.Course.CreatorId == currentUserId)
            .ExecuteUpdateAsync(r => r.SetProperty(r => r.IsPublished, isPublished));
    }

    public async Task<bool> CheckOwnerAsync(long resourceId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.CourseResources
            .AsNoTracking()
            .Where(c => c.Id == resourceId && c.Module.Course.CreatorId == currentUserId)
            .AnyAsync();
    }

    static public async Task<List<long>> GetCourseResourceIdsAsync(AppDbContext db, long courseId)
    {
        var moduleIds = await db.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId)
            .Select(m => m.Id)
            .ToListAsync();
        return await db.CourseResources
            .AsNoTracking()
            .Where(r => moduleIds.Contains(r.ModuleId))
            .Select(r => r.Id)
            .ToListAsync();
    }

    static public async Task<CourseProgress> GetCourseProgressAsync(AppDbContext db, long courseId, long userId)
    {
        var resourceIds = await GetCourseResourceIdsAsync(db, courseId);
        var completed = await db.ResourceProgress
            .AsNoTracking()
            .Where(r => r.UserId == userId && r.IsCompleted && resourceIds.Contains(r.ResourceId))
            .CountAsync();
        return new CourseProgress(Completed: completed, Total: resourceIds.Count);
    }

    static public async Task<CourseResource> CreateResourceAsync(AppDbContext db, long moduleId, ResourceRequestInfo info, ResourceType type, CancellationToken ct = default)
    {
        var resource = new CourseResource
        {
            ModuleId = moduleId,
            Type = type,
            Title = info.Title,
            OrderIndex = info.OrderIndex,
            IsPublished = info.IsPublished,
            AccessPassword = info.AccessPassword
        };

        db.CourseResources.Add(resource);
        await db.SaveChangesAsync(ct);
        return resource;
    }

    static public async Task<CourseResource> UpdateResourceAsync(AppDbContext db, long resourceId, ResourceRequestInfo info)
    {
        var resource = await db.CourseResources.FirstOrDefaultAsync(r => r.Id == resourceId);
        if (resource is not null)
        {
            resource.Title = info.Title;
            resource.OrderIndex = info.OrderIndex;
            resource.IsPublished = info.IsPublished;
            resource.AccessPassword = info.AccessPassword;

            db.CourseResources.Update(resource);
            await db.SaveChangesAsync();

            return resource;
        }

        throw new Exception($"UpdateResourceAsync: Resource id {resourceId} does not exist.");
    }
}
