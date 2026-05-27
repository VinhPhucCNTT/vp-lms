using Backend.Core.Common;
using Backend.Data;
using Backend.Services.Modules.Types;
using Backend.Core.Entities.Courses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Modules.Services;

public class CModuleService(
    IDbContextFactory<AppDbContext> dbFactory
) : ICModuleService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

    public async Task<MResult<CModuleResponse, CModuleGetError>> GetModuleByIdAsync(Guid userId, Guid moduleId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var module = await db.Modules.Include(m => m.Course).FirstOrDefaultAsync(m => m.Id == moduleId);

        if (module == null)
            return MResult<CModuleResponse, CModuleGetError>
                .Failure(CModuleGetError.NotFound);
        if (module.Course.CreatorId != userId)
            return MResult<CModuleResponse, CModuleGetError>
                .Failure(CModuleGetError.Unauthorized);

        return MResult<CModuleResponse, CModuleGetError>.Success(
            new CModuleResponse(module.Title, module.Description, module.OrderIndex, module.IsPublished));
    }

    public async Task<QueryResponse<CModuleResponse>> QueryModulesAsync(Guid userId, Guid courseId, CModuleRequest query)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var modules = db.Modules.Include(m => m.Course).Where(m => m.CourseId == courseId && m.Course.CreatorId == userId);

        if (!await modules.AnyAsync())
            return new QueryResponse<CModuleResponse>(query.PageNumber, query.PageSize, 0, []);

        if (!string.IsNullOrEmpty(query.Title))
            modules = modules.Where(m => m.Title.Equals(query.Title, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.Description))
            modules = modules.Where(m => m.Description != null && m.Description.Equals(query.Description, StringComparison.OrdinalIgnoreCase));

        if (query.IsPublished != null)
            modules = modules.Where(m => m.IsPublished == query.IsPublished);

        var list = await modules
            .Select(m => new CModuleResponse(
                m.Title,
                m.Description,
                m.OrderIndex,
                m.IsPublished
            ))
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new QueryResponse<CModuleResponse>(query.PageNumber, query.PageSize, await modules.CountAsync(), list);
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
