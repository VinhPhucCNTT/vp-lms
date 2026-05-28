using Backend.Data;
using Backend.Core.Types;
using Microsoft.EntityFrameworkCore;
using Backend.Services.Common;

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
        return await db.Modules
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
        return await db.Modules
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

        return await db.Modules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId && !m.IsPublished)
            .Select(m => new ModuleResponse(
                m.Title,
                m.OrderIndex))
            .ToListAsync();
    }

    public async Task<bool> AddModuleAsync(long courseId, ModuleSetRequest dto) {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (!await IsUserValidAsync(db, courseId))
            return false;

        var module = new Mo
        {
            Title = dto.Title,
        };
    }

    // On failure: ignore and go next
    public async Task<bool> PublishModulesAsync(List<long> moduleId) { }

    // On failure: ignore and go next
    public async Task<bool> UnpublishModulesAsync(List<long> moduleId) { }

    public async Task<bool> UpdateModuleAsync(long moduleId, ModuleSetRequest dto) { }

    // On failure: interrupt
    public async Task<bool> DeleteModulesAsync(List<long> moduleId) { }

    private async Task<bool> IsUserValidAsync(AppDbContext db, long courseId) {
        var courseCreator = await db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => c.CreatorId)
            .FirstOrDefaultAsync();

        return _currentUserService.UserId != courseCreator;
    }
}
