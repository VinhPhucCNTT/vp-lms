using Backend.Core.Common;
using Backend.Data;
using Backend.Core.Types;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Courses;

public class ModuleService(
    IDbContextFactory<AppDbContext> dbFactory
)
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

    public async Task<MResult<CModuleSetResponse, CModuleSetError>> AddModuleAsync(Guid userId, Guid courseId, CModuleSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return MResult<CModuleSetResponse, CModuleSetError>
                .Failure(CModuleSetError.InvalidRequest);
        if (course.CreatorId != userId)
            return MResult<CModuleSetResponse, CModuleSetError>
                .Failure(CModuleSetError.Unauthorized);

        var module = new Module
        {
            CourseId = courseId,
            Title = dto.Title,
            Description = dto.Description,
            OrderIndex = dto.OrderIndex,
            IsPublished = dto.IsPublished
        };
        db.Modules.Add(module);
        await db.SaveChangesAsync();

        return MResult<CModuleSetResponse, CModuleSetError>
            .Success(new CModuleSetResponse(
                module.Title,
                module.Description,
                module.OrderIndex,
                module.IsPublished
            ));
    }

    public async Task<MResult<CModuleSetResponse, CModuleSetError>> UpdateModuleAsync(Guid userId, Guid courseId, Guid moduleId, CModuleSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var module = await db.Modules.Include(m => m.Course).FirstOrDefaultAsync(m => m.CourseId == courseId);

        if (module == null)
            return MResult<CModuleSetResponse, CModuleSetError>
                .Failure(CModuleSetError.InvalidRequest);
        if (module.Course.CreatorId != userId)
            return MResult<CModuleSetResponse, CModuleSetError>
                .Failure(CModuleSetError.Unauthorized);

        module.CourseId = courseId;
        module.Title = dto.Title;
        module.Description = dto.Description;
        module.OrderIndex = dto.OrderIndex;
        module.IsPublished = dto.IsPublished;

        db.Modules.Update(module);
        await db.SaveChangesAsync();

        return MResult<CModuleSetResponse, CModuleSetError>
            .Success(new CModuleSetResponse(
                module.Title,
                module.Description,
                module.OrderIndex,
                module.IsPublished
            ));
    }

    public async Task<CModuleDeleteStatus> DeleteModuleAsync(Guid userId, Guid courseId, Guid moduleId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var module = await db.Modules.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == moduleId);

        if (module == null || module.CourseId != courseId)
            return CModuleDeleteStatus.InvalidRequest;
        if (module.Course.CreatorId != userId)
            return CModuleDeleteStatus.Unauthorized;

        db.Modules.Remove(module);
        await db.SaveChangesAsync();

        return CModuleDeleteStatus.Success;
    }
}
