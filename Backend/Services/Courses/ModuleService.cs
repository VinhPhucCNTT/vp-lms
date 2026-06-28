using Backend.Data;
using Backend.Core.Types;
using Microsoft.EntityFrameworkCore;
using Backend.Services.Common;
using Backend.Core.Entities.Courses;
using Sqids;
using AutoMapper;

namespace Backend.Services.Courses;

public class ModuleService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper,
    SqidsEncoder<long> sqidsEncoder)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly SqidsEncoder<long> _sqidsEncoder = sqidsEncoder;
    private readonly IMapper _mapper = mapper;

    public async Task<ModuleDetailResponse?> GetModuleByIdAsync(long moduleId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.CourseModules
            .AsNoTracking()
            .Where(m => m.Id == moduleId)
            .Where(m => m.IsPublished || m.Course.CreatorId == currentUserId)
            .Select(m => _mapper.Map<ModuleDetailResponse>(m))
            .FirstOrDefaultAsync();
    }

    public async Task<List<ModuleResponse>> GetPublishedModulesAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId && m.IsPublished)
            .Select(m => _mapper.Map<ModuleResponse>(m))
            .ToListAsync();
    }

    public async Task<List<ModuleResponse>> GetUnpublishedModulesAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId && !m.IsPublished && m.Course.CreatorId == currentUserId)
            .Select(m => _mapper.Map<ModuleResponse>(m))
            .ToListAsync();
    }

    public async Task<ModuleSetResponse> CreateModuleAsync(long courseId, ModuleSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var module = new CourseModule
        {
            CourseId = courseId,
            Title = dto.Title,
            Description = dto.Description,
            OrderIndex = dto.OrderIndex,
            IsPublished = dto.IsPublished
        };
        db.CourseModules.Add(module);
        await db.SaveChangesAsync();

        return _mapper.Map<ModuleSetResponse>(module);
    }

    public async Task<ModuleSetResponse?> UpdateModuleAsync(long moduleId, ModuleSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var module = await db.CourseModules.FirstOrDefaultAsync(m => m.Id == moduleId);
        if (module == null)
            return null;

        module.Title = dto.Title;
        module.Description = dto.Description;
        module.OrderIndex = dto.OrderIndex;
        module.IsPublished = dto.IsPublished;
        await db.SaveChangesAsync();

        return _mapper.Map<ModuleSetResponse>(module);
    }

    public async Task<bool> SetModulePublishStatusAsync(long courseId, long moduleId, bool isPublished)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var count = await db.CourseModules
            .Where(m => m.CourseId == courseId && m.IsPublished != isPublished)
            .Where(m => m.Id == moduleId)
            .ExecuteUpdateAsync(m => m.SetProperty(m => m.IsPublished, isPublished));
        return count > 0;
    }

    public async Task<int> SetModulesPublishStatusAsync(long courseId, List<long> moduleIds, bool isPublished)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.CourseModules
            .Where(m => m.CourseId == courseId && m.IsPublished != isPublished && m.Course.CreatorId == currentUserId)
            .Where(m => moduleIds.Contains(m.Id))
            .ExecuteUpdateAsync(m => m.SetProperty(m => m.IsPublished, isPublished));
    }

    public async Task<bool> DeleteModuleAsync(long moduleId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var module = await db.CourseModules.FirstOrDefaultAsync(m => m.Id == moduleId);
        if (module is null)
            return false;
        db.CourseModules.Remove(module);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteModulesAsync(List<long> moduleIds)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        var modules = db.CourseModules
            .Where(m => moduleIds.Contains(m.Id) && m.Course.CreatorId == currentUserId);
        var count = await modules.CountAsync();
        db.CourseModules.RemoveRange(modules);
        await db.SaveChangesAsync();
        return count;
    }

    // TODO: Reorder multiple modules at once
    public async Task<bool> ReorderModuleAsync(long moduleId, int orderIndex)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        await using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var target = await db.CourseModules.FirstOrDefaultAsync(m => m.Id == moduleId);
            if (target == null) return false;

            var modules = db.CourseModules
                .Where(m => m.CourseId == target.CourseId);
            var count = await modules.CountAsync();

            if (orderIndex < 0) orderIndex = 0;
            orderIndex = Math.Min(orderIndex, count + 1);

            await modules
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

    public async Task GetModuleProgressAsync(long moduleId)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }

    private async Task<bool> CanDeleteModuleAsync(CourseModule module)
    {
        // TODO: Internally track if courses (and consequently it's dependents) can be deleted, otherwise archirve it, keeping all records instead.
        // => Implement CourseService.CanBeDeletedAsync()
        return true;
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
