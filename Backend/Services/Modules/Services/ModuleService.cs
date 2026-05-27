using Backend.Core.Common;
using Backend.Data;
using Backend.Services.Modules.Types;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Modules.Services;

public class ModuleService(
    IDbContextFactory<AppDbContext> dbFactory
) : IModuleService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

    public async Task<ModuleResponse?> GetModuleByIdAsync(Guid moduleId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var module = await db.Modules.FirstOrDefaultAsync(m => m.Id == moduleId);

        return (module == null) ? null :
            new ModuleResponse(
                module.Title,
                module.Description,
                module.OrderIndex
            );
    }

    public async Task<QueryResponse<ModuleResponse>?> QueryModulesAsync(Guid courseId, ModuleRequest query)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (!await db.Courses.AnyAsync(c => c.Id == courseId))
            return null;
        var modules = db.Modules.Where(m => m.CourseId == courseId).AsQueryable();

        if (!string.IsNullOrEmpty(query.Title))
            modules = modules.Where(m => m.Title.Equals(query.Title, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.Description))
            modules = modules.Where(m => m.Description != null && m.Description.Equals(query.Description, StringComparison.OrdinalIgnoreCase));

        var list = await modules
            .Select(m => new ModuleResponse(
                m.Title,
                m.Description,
                m.OrderIndex))
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new QueryResponse<ModuleResponse>(query.PageNumber, query.PageSize, await modules.CountAsync(), list);
    }
}
