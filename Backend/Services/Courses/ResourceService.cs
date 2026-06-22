using AutoMapper;
using Backend.Core.Entities.Courses;
using Backend.Core.Types;
using Backend.Data;
using Backend.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Courses;

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
        return await db.ModuleResources
            .AsNoTracking()
            .Where(r => r.Id == resourceId)
            .Where(r => r.IsPublished || r.Module.Course.CreatorId == currentUserId)
            .Select(r => _mapper.Map<ResourceDetailResponse>(r))
            .FirstOrDefaultAsync();
    }

    public async Task<List<ResourceResponse>> GetPublishedResourcesAsync(long moduleId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.ModuleResources
            .AsNoTracking()
            .Where(r => r.ModuleId == moduleId && r.IsPublished)
            .Select(r => _mapper.Map<ResourceResponse>(r))
            .ToListAsync();
    }

    public async Task<List<ResourceResponse>> GetUnpublishedResourcesAsync(long moduleId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.ModuleResources
            .AsNoTracking()
            .Where(r => r.ModuleId == moduleId && !r.IsPublished && r.Module.Course.CreatorId == currentUserId)
            .Select(r => _mapper.Map<ResourceResponse>(r))
            .ToListAsync();
    }

    public async Task<ResourceSetResponse?> CreateResourceAsync(long moduleId, ResourceCreateRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var resource = new ModuleResource
        {
            ModuleId = moduleId,
            ResourceType = dto.Type,
            Title = dto.Title,
            Description = dto.Description,
            OrderIndex = dto.OrderIndex,
            IsPublished = dto.IsPublished,
            AvailableFrom = dto.AvailableFrom,
            AvailableUntil = dto.AvailableUntil,
            AccessPassword = dto.AccessPassword
        };

        // TODO: Call the appropriate create methods
        // switch (dto.type)
        // {
        // }

        db.ModuleResources.Add(resource);
        await db.SaveChangesAsync();

        return _mapper.Map<ResourceSetResponse>(resource);
    }

    public async Task<ResourceSetResponse?> UpdateResourceAsync(long resourceId, ResourceUpdateRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var resource = await db.ModuleResources.FirstOrDefaultAsync(r => r.Id == resourceId);
        if (resource == null)
            return null;

        resource.Title = dto.Title;
        resource.Description = dto.Description;
        resource.OrderIndex = dto.OrderIndex;
        resource.IsPublished = dto.IsPublished;
        resource.AvailableFrom = dto.AvailableFrom;
        resource.AvailableUntil = dto.AvailableUntil;
        resource.AccessPassword = dto.AccessPassword;
        await db.SaveChangesAsync();

        return _mapper.Map<ResourceSetResponse>(resource);
    }

    public async Task<bool> DeleteResourceAsync(long resourceId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var count = await db.ModuleResources.Where(r => r.Id == resourceId).ExecuteDeleteAsync();
        return count > 0;
    }

    public async Task<bool> ReorderResourceAsync(long resourceId, int orderIndex)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var target = await db.ModuleResources.FirstOrDefaultAsync(m => m.Id == resourceId);
            if (target == null) return false;

            var resources = db.ModuleResources
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

    public async Task<bool> SetResourcePublishStatusAsync(long moduleId, long resourceId, bool isPublished)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var count = await db.ModuleResources
            .Where(r => r.ModuleId == moduleId && r.Id == resourceId)
            .ExecuteUpdateAsync(r => r.SetProperty(r => r.IsPublished, isPublished));

        return count > 0;
    }

    public async Task<int> SetResourcesPublishStatusAsync(long moduleId, List<long> resourceIds, bool isPublished)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.ModuleResources
            .Where(r => r.ModuleId == moduleId && resourceIds.Contains(r.Id) && r.Module.Course.CreatorId == currentUserId)
            .ExecuteUpdateAsync(r => r.SetProperty(r => r.IsPublished, isPublished));
    }

    public async Task<bool> CheckOwnerAsync(long moduleId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.CourseModules
            .AsNoTracking()
            .Where(c => c.Id == moduleId && c.Course.CreatorId == currentUserId)
            .AnyAsync();
    }
}
