using AutoMapper;
using Backend.Api.Core.Types;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Courses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Courses;

public interface IModuleService
{
    public Task<List<ModuleResponse>> GetCourseModulesAsync(long courseId, CancellationToken ct = default);

    public Task<ModuleSetResponse> CreateModuleAsync(long courseId, ModuleSetRequest dto, CancellationToken ct = default);

    public Task<ModuleSetResponse?> UpdateModuleAsync(long moduleId, ModuleSetRequest dto, CancellationToken ct = default);

    public Task<bool> SetModulePublishStatusAsync(long courseId, long moduleId, bool isPublished, CancellationToken ct = default);

    public Task<bool> DeleteModuleAsync(long moduleId, CancellationToken ct = default);

    public Task<bool> ReorderModuleAsync(long moduleId, int orderIndex, CancellationToken ct = default);

    public Task GetModuleProgressAsync(long moduleId, CancellationToken ct = default);
}

public class ModuleService(
    IDbContextFactory<AppDbContext> dbFactory,
    IMapper mapper) : IModuleService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly IMapper _mapper = mapper;

    public async Task<List<ModuleResponse>> GetCourseModulesAsync(long courseId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.CourseModules
            .AsNoTracking()
            .Where(m => m.CourseId == courseId)
            .Select(m => _mapper.Map<ModuleResponse>(m))
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<ModuleSetResponse> CreateModuleAsync(long courseId, ModuleSetRequest dto, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var module = new CourseModule
        {
            CourseId = courseId,
            Title = dto.Title,
            Description = dto.Description,
            OrderIndex = dto.OrderIndex,
            IsPublished = dto.IsPublished
        };
        db.CourseModules.Add(module);
        await db.SaveChangesAsync(ct);

        return _mapper.Map<ModuleSetResponse>(module);
    }

    public async Task<ModuleSetResponse?> UpdateModuleAsync(long moduleId, ModuleSetRequest dto, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var module = await db.CourseModules.FirstOrDefaultAsync(m => m.Id == moduleId, cancellationToken: ct);
        if (module == null)
            return null;

        module.Title = dto.Title;
        module.Description = dto.Description;
        module.OrderIndex = dto.OrderIndex;
        module.IsPublished = dto.IsPublished;
        await db.SaveChangesAsync(ct);

        return _mapper.Map<ModuleSetResponse>(module);
    }

    public async Task<bool> SetModulePublishStatusAsync(long courseId, long moduleId, bool isPublished, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var count = await db.CourseModules
            .Where(m => m.CourseId == courseId && m.IsPublished != isPublished)
            .Where(m => m.Id == moduleId)
            .ExecuteUpdateAsync(m => m.SetProperty(m => m.IsPublished, isPublished), cancellationToken: ct);
        return count > 0;
    }

    public async Task<bool> DeleteModuleAsync(long moduleId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var module = await db.CourseModules.FirstOrDefaultAsync(m => m.Id == moduleId, cancellationToken: ct);
        if (module is null)
            return false;
        db.CourseModules.Remove(module);
        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<bool> ReorderModuleAsync(long moduleId, int orderIndex, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            var target = await db.CourseModules.FirstOrDefaultAsync(m => m.Id == moduleId, cancellationToken: ct);
            if (target == null) return false;

            var modules = db.CourseModules
                .Where(m => m.CourseId == target.CourseId);
            var count = await modules.CountAsync(cancellationToken: ct);

            if (orderIndex < 0) orderIndex = 0;
            orderIndex = Math.Min(orderIndex, count + 1);

            await modules
                .Where(m => m.OrderIndex >= orderIndex)
                .ExecuteUpdateAsync(m => m.SetProperty(m => m.OrderIndex, m => m.OrderIndex + 1), cancellationToken: ct);
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

    public async Task GetModuleProgressAsync(long moduleId, CancellationToken ct = default)
    {
        // TODO: Implement
        throw new NotImplementedException();
    }

    static private async Task<bool> CanDeleteModuleAsync(CourseModule module, CancellationToken ct = default)
    {
        // TODO: Internally track if courses (and consequently it's dependents) can be deleted, otherwise archirve it, keeping all records instead.
        // => Implement CourseService.CanBeDeletedAsync()
        return true;
    }
}
