using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Core.Types;
using Backend.Services.Courses;
using Microsoft.AspNetCore.Mvc;
using Sqids;

namespace Backend.Endpoints;

public static class ModuleEndpoints
{
    public static void AddModuleEndpoints(this IEndpointRouteBuilder route)
    {
        var module = route.MapGroup("/api/module").WithTags("Modules");

        module.MapGet("{moduleSqid}", HandleGetById);
        module.MapGet("published/{courseSqid}", HandleGetPublished).RequireAuthorization();
        module.MapGet("unpublished/{courseSqid}", HandleGetUnpublished).RequireAuthorization();
        module.MapGet("{moduleSqid}/check", HandleCheckOwner).RequireAuthorization();

        module.MapPut("{courseSqid}", HandleCreate).RequireAuthorization();
        module.MapPost("{moduleSqid}", HandleUpdate).RequireAuthorization();
        module.MapDelete("{moduleSqid}", HandleDelete).RequireAuthorization();
        module.MapPost("bulk-delete", HandleBulkDelete).RequireAuthorization();

        module.MapPost("{courseSqid}/publish/{moduleSqid}", HandlePublish).RequireAuthorization();
        module.MapPost("{courseSqid}/unpublish/{moduleSqid}", HandleUnpublish).RequireAuthorization();
        module.MapPost("{courseSqid}/bulk-publish", HandleBulkPublish).RequireAuthorization();
        module.MapPost("{courseSqid}/bulk-unpublish", HandleBulkUnpublish).RequireAuthorization();
        module.MapPost("{moduleSqid}/reorder", HandleReorder).RequireAuthorization();
    }

    private static async
        Task<Results<Ok<ModuleDetailResponse>, NotFound>>
        HandleGetById(
            [FromRoute] string moduleSqid,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();
        var result = await moduleService.GetModuleByIdAsync(moduleId);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Ok<List<ModuleResponse>>>
        HandleGetPublished(
            [FromRoute] string courseSqid,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var courseId = sqidsEncoder.Decode(courseSqid).Single();
        return TypedResults.Ok(
            await moduleService.GetPublishedModulesAsync(courseId));
    }

    private static async
        Task<Ok<List<ModuleResponse>>>
        HandleGetUnpublished(
            [FromRoute] string courseSqid,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var courseId = sqidsEncoder.Decode(courseSqid).Single();
        return TypedResults.Ok(
            await moduleService.GetUnpublishedModulesAsync(courseId));
    }

    private static async
        Task<Ok<bool>>
        HandleCheckOwner(
            [FromRoute] string moduleSqid,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();
        return TypedResults.Ok(
            await moduleService.CheckOwnerAsync(moduleId));
    }

    private static async
        Task<Ok<ModuleSetResponse>>
        HandleCreate(
            [FromRoute] string courseSqid,
            [FromBody] ModuleSetRequest request,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var courseId = sqidsEncoder.Decode(courseSqid).Single();
        return TypedResults.Ok(
            await moduleService.CreateModuleAsync(courseId, request));
    }

    private static async
        Task<Results<Ok<ModuleSetResponse>, NotFound, UnauthorizedHttpResult>>
        HandleUpdate(
            [FromRoute] string moduleSqid,
            [FromBody] ModuleSetRequest request,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();
        if (!await moduleService.CheckOwnerAsync(moduleId))
            return TypedResults.Unauthorized();

        var result = await moduleService.UpdateModuleAsync(moduleId, request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, UnauthorizedHttpResult>>
        HandleDelete(
            [FromRoute] string moduleSqid,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();
        if (!await moduleService.CheckOwnerAsync(moduleId))
            return TypedResults.Unauthorized();

        return await moduleService.DeleteModuleAsync(moduleId)
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<int>, NotFound>>
        HandleBulkDelete(
            [FromBody] List<string> moduleSqids,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        if (moduleSqids is null || moduleSqids.Count == 0)
            return TypedResults.Ok(0);
        var moduleIds = moduleSqids
            .Select(sqid => sqidsEncoder.Decode(sqid).SingleOrDefault())
            .ToList();

        var count = await moduleService.DeleteModulesAsync(moduleIds);
        return count > 0
            ? TypedResults.Ok(count)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, UnauthorizedHttpResult>>
        HandlePublish(
            [FromRoute] string courseSqid,
            [FromRoute] string moduleSqid,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();
        var courseId = sqidsEncoder.Decode(courseSqid).Single();
        if (!await moduleService.CheckOwnerAsync(moduleId))
            return TypedResults.Unauthorized();

        var result = await moduleService.SetModulePublishStatusAsync(courseId, moduleId, true);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, UnauthorizedHttpResult>>
        HandleUnpublish(
            [FromRoute] string courseSqid,
            [FromRoute] string moduleSqid,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();
        var courseId = sqidsEncoder.Decode(courseSqid).Single();
        if (!await moduleService.CheckOwnerAsync(moduleId))
            return TypedResults.Unauthorized();

        var result = await moduleService.SetModulePublishStatusAsync(courseId, moduleId, false);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<int>, NotFound, UnauthorizedHttpResult>>
        HandleBulkPublish(
            [FromRoute] string courseSqid,
            [FromBody] List<string> moduleSqids,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        if (moduleSqids is null || moduleSqids.Count == 0)
            return TypedResults.Ok(0);
        var moduleIds = moduleSqids
            .Select(sqid => sqidsEncoder.Decode(sqid).SingleOrDefault())
            .ToList();
        var courseId = sqidsEncoder.Decode(courseSqid).Single();

        var count = await moduleService.SetModulesPublishStatusAsync(courseId, moduleIds, true);
        return count > 0
            ? TypedResults.Ok(count)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<int>, NotFound, UnauthorizedHttpResult>>
        HandleBulkUnpublish(
            [FromRoute] string courseSqid,
            [FromBody] List<string> moduleSqids,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        if (moduleSqids is null || moduleSqids.Count == 0)
            return TypedResults.Ok(0);
        var moduleIds = moduleSqids
            .Select(sqid => sqidsEncoder.Decode(sqid).SingleOrDefault())
            .ToList();
        var courseId = sqidsEncoder.Decode(courseSqid).Single();

        var count = await moduleService.SetModulesPublishStatusAsync(courseId, moduleIds, false);
        return count > 0
            ? TypedResults.Ok(count)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, UnauthorizedHttpResult>>
        HandleReorder(
            [FromRoute] string moduleSqid,
            [FromQuery] int orderIndex,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();
        if (!await moduleService.CheckOwnerAsync(moduleId))
            return TypedResults.Unauthorized();

        return await moduleService.ReorderModuleAsync(moduleId, orderIndex)
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }
}
