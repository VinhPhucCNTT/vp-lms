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

        module.MapGet("{moduleId}", HandleGetById);
        module.MapGet("published/{courseId}", HandleGetPublished).RequireAuthorization();
        module.MapGet("unpublished/{courseId}", HandleGetUnpublished).RequireAuthorization();
        module.MapGet("{moduleId}/check", HandleCheckOwner).RequireAuthorization();

        module.MapPut("{courseId}", HandleCreate).RequireAuthorization();
        module.MapPost("{moduleId}", HandleUpdate).RequireAuthorization();
        module.MapDelete("{moduleId}", HandleDelete).RequireAuthorization();
        module.MapPost("bulk-delete", HandleBulkDelete).RequireAuthorization();

        module.MapPost("{courseId}/publish/{moduleId}", HandlePublish).RequireAuthorization();
        module.MapPost("{courseId}/unpublish/{moduleId}", HandleUnpublish).RequireAuthorization();
        module.MapPost("{courseId}/bulk-publish", HandleBulkPublish).RequireAuthorization();
        module.MapPost("{courseId}/bulk-unpublish", HandleBulkUnpublish).RequireAuthorization();
        module.MapPost("{moduleId}/reorder", HandleReorder).RequireAuthorization();
    }

    private static async
        Task<Results<Ok<ModuleDetailResponse>, BadRequest, NotFound>>
        HandleGetById(
            string moduleId,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var decoded = sqidsEncoder.Decode(moduleId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await moduleService.GetModuleByIdAsync(decoded[0]);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<List<ModuleResponse>>, BadRequest>>
        HandleGetPublished(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(
            await moduleService.GetPublishedModulesAsync(decoded[0]));
    }

    private static async
        Task<Results<Ok<List<ModuleResponse>>, BadRequest>>
        HandleGetUnpublished(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(
            await moduleService.GetUnpublishedModulesAsync(decoded[0]));
    }

    private static async
        Task<Results<Ok<bool>, BadRequest>>
        HandleCheckOwner(
            string moduleId,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var decoded = sqidsEncoder.Decode(moduleId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(
            await moduleService.CheckOwnerAsync(decoded[0]));
    }

    private static async
        Task<Results<Ok<ModuleSetResponse>, BadRequest>>
        HandleCreate(
            string courseId,
            [FromBody] ModuleSetRequest request,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(
            await moduleService.CreateModuleAsync(decoded[0], request));
    }

    private static async
        Task<Results<Ok<ModuleSetResponse>, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleUpdate(
            string moduleId,
            [FromBody] ModuleSetRequest request,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var decoded = sqidsEncoder.Decode(moduleId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        if (!await moduleService.CheckOwnerAsync(decoded[0]))
            return TypedResults.Unauthorized();

        var result = await moduleService.UpdateModuleAsync(decoded[0], request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleDelete(
            string moduleId,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var decoded = sqidsEncoder.Decode(moduleId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        if (!await moduleService.CheckOwnerAsync(decoded[0]))
            return TypedResults.Unauthorized();

        return await moduleService.DeleteModuleAsync(decoded[0])
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<int>, BadRequest, NotFound>>
        HandleBulkDelete(
            [FromBody] List<string> moduleIds,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        if (moduleIds is null || moduleIds.Count == 0)
            return TypedResults.Ok(0);

        List<long> decodedIds = [];
        foreach (var id in moduleIds)
        {
            var decoded = sqidsEncoder.Decode(id);
            if (decoded.Count != 1)
                return TypedResults.BadRequest();
            decodedIds.Add(decoded[0]);
        }

        var count = await moduleService.DeleteModulesAsync(decodedIds);
        return count > 0
            ? TypedResults.Ok(count)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandlePublish(
            string courseId,
            string moduleId,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var dModuleId = sqidsEncoder.Decode(moduleId);
        var dCourseId = sqidsEncoder.Decode(courseId);
        if (dModuleId.Count != 1 || dCourseId.Count != 1)
            return TypedResults.BadRequest();

        if (!await moduleService.CheckOwnerAsync(dModuleId[0]))
            return TypedResults.Unauthorized();

        var result = await moduleService.SetModulePublishStatusAsync(dCourseId[0], dModuleId[0], true);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleUnpublish(
            string courseId,
            string moduleId,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var dModuleId = sqidsEncoder.Decode(moduleId);
        var dCourseId = sqidsEncoder.Decode(courseId);
        if (dModuleId.Count != 1 || dCourseId.Count != 1)
            return TypedResults.BadRequest();

        if (!await moduleService.CheckOwnerAsync(dModuleId[0]))
            return TypedResults.Unauthorized();

        var result = await moduleService.SetModulePublishStatusAsync(dCourseId[0], dModuleId[0], false);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<int>, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleBulkPublish(
            string courseId,
            [FromBody] List<string> moduleIds,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        if (moduleIds is null || moduleIds.Count == 0)
            return TypedResults.Ok(0);

        var dCourseId = sqidsEncoder.Decode(courseId);
        if (dCourseId.Count != 1)
            return TypedResults.BadRequest();

        List<long> dModuleIds = [];
        foreach (var id in moduleIds)
        {
            var decoded = sqidsEncoder.Decode(id);
            if (decoded.Count != 1)
                return TypedResults.BadRequest();
            dModuleIds.Add(decoded[0]);
        }

        var count = await moduleService.SetModulesPublishStatusAsync(dCourseId[0], dModuleIds, true);
        return count > 0
            ? TypedResults.Ok(count)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<int>, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleBulkUnpublish(
            string courseId,
            [FromBody] List<string> moduleIds,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        if (moduleIds is null || moduleIds.Count == 0)
            return TypedResults.Ok(0);

        var dCourseId = sqidsEncoder.Decode(courseId);
        if (dCourseId.Count != 1)
            return TypedResults.BadRequest();

        List<long> dModuleIds = [];
        foreach (var id in moduleIds)
        {
            var decoded = sqidsEncoder.Decode(id);
            if (decoded.Count != 1)
                return TypedResults.BadRequest();
            dModuleIds.Add(decoded[0]);
        }

        var count = await moduleService.SetModulesPublishStatusAsync(dCourseId[0], dModuleIds, false);
        return count > 0
            ? TypedResults.Ok(count)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleReorder(
            string moduleId,
            [FromQuery] int orderIndex,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var decoded = sqidsEncoder.Decode(moduleId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        if (!await moduleService.CheckOwnerAsync(decoded[0]))
            return TypedResults.Unauthorized();

        return await moduleService.ReorderModuleAsync(decoded[0], orderIndex)
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }
}
