using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
using Backend.Api.Services.Courses;
using Microsoft.AspNetCore.Mvc;
using Sqids;

namespace Backend.Api.Endpoints.Course;

public static class ModuleEndpoints
{
    public static void AddModuleEndpoints(this IEndpointRouteBuilder route)
    {
        var module = route.MapGroup("/api/module").WithTags("Modules");

        module.MapPut("{moduleId}", HandleUpdate).RequireAuthorization();
        module.MapDelete("{moduleId}", HandleDelete).RequireAuthorization();

        module.MapPost("publish/{moduleId}", HandlePublish).RequireAuthorization();
        module.MapPost("unpublish/{moduleId}", HandleUnpublish).RequireAuthorization();
        module.MapPost("reorder/{moduleId}", HandleReorder).RequireAuthorization();
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

        return await moduleService.DeleteModuleAsync(decoded[0])
            ? TypedResults.Ok()
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

        var result = await moduleService.SetModulePublishStatusAsync(dCourseId[0], dModuleId[0], false);
        return result
            ? TypedResults.Ok()
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

        return await moduleService.ReorderModuleAsync(decoded[0], orderIndex)
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }
}
