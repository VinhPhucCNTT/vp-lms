using Backend.Data;
using Backend.Core.Types;
using Microsoft.EntityFrameworkCore;
using Backend.Services.Common;
using Backend.Core.Entities.Courses;

namespace Backend.Services.Courses;

public class ModuleService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService
)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;

    public async Task<ModuleDetailResponse?> GetModuleByIdAsync(long moduleId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.CourseModules
            .AsNoTracking()
            .Where(m => m.Id == moduleId)
            .Where(m => m.IsPublished || m.Course.CreatorId == currentUserId)
            .Select(m => new ModuleDetailResponse(
                m.Title,
                m.Description,
                m.OrderIndex))
            .FirstOrDefaultAsync();
    }

    public async Task<List<ModuleResponse>> GetPublishedModulesAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId && m.IsPublished)
            .Select(m => new ModuleResponse(
                m.Title,
                m.OrderIndex))
            .ToListAsync();
    }

    public async Task<List<ModuleResponse>> GetUnpublishedModulesAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (!await IsUserValidAsync(db, courseId))
            return [];

        return await db.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId && !m.IsPublished)
            .Select(m => new ModuleResponse(
                m.Title,
                m.OrderIndex))
            .ToListAsync();
    }

    public async Task<bool> AddModuleAsync(long courseId, ModuleSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (!await IsUserValidAsync(db, courseId))
            return false;

        var module = new CourseModule
        {
            Title = dto.Title,
            Description = dto.Description,
            OrderIndex = dto.OrderIndex,
            IsPublished = dto.IsPublished
        };
        db.CourseModules.Add(module);
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<int> PublishModulesAsync(long courseId, List<long> moduleIds)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (moduleIds == null || moduleIds.Count == 0 || !await IsUserValidAsync(db, courseId))
            return 0;

        return await db.CourseModules
            .Where(m => m.CourseId == courseId && !m.IsPublished)
            .Where(m => moduleIds.Contains(m.Id))
            .ExecuteUpdateAsync(m => m.SetProperty(m => m.IsPublished, true));
    }

    public async Task<int> UnpublishModulesAsync(long courseId, List<long> moduleIds)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (moduleIds == null || moduleIds.Count == 0 || !await IsUserValidAsync(db, courseId))
            return 0;

        return await db.CourseModules
            .Where(m => m.CourseId == courseId && m.IsPublished)
            .Where(m => moduleIds.Contains(m.Id))
            .ExecuteUpdateAsync(m => m.SetProperty(m => m.IsPublished, false));
    }

    public async Task<bool> UpdateModuleAsync(long moduleId, ModuleSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var module = await db.CourseModules.FirstOrDefaultAsync(m => m.Id == moduleId);
        if (module == null || !await IsUserValidAsync(db, module.CourseId))
            return false;

        module.Title = dto.Title;
        module.Description = dto.Description;
        module.OrderIndex = dto.OrderIndex;
        module.IsPublished = dto.IsPublished;
        await db.SaveChangesAsync();

        return true;
    }

    public async Task<int> DeleteModulesAsync(List<long> moduleIds)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.CourseModules
            .Where(m => moduleIds.Contains(m.Id))
            .ExecuteDeleteAsync();
    }

    private async Task<bool> CanDeleteModuleAsync(CourseModule module)
    {
        // TODO: Internally track if courses (and consequently it's dependents) can be deleted, otherwise archirve it, keeping all records instead.
        // => Implement CourseService.CanBeDeletedAsync()
        // => New Course entity flag: CanBeDeleted
        return true;
    }

    private async Task<bool> IsUserValidAsync(AppDbContext db, long courseId)
    {
        var courseCreator = await db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => c.CreatorId)
            .FirstOrDefaultAsync();

        return _currentUserService.UserId != courseCreator;
    }
}
