using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
using Backend.Api.Services.Courses;
using Microsoft.AspNetCore.Mvc;
using Sqids;

namespace Backend.Api.Endpoints.Course;

public static class ResourceEndpoints
{
    public static void AddResourceEndpoints(this IEndpointRouteBuilder route)
    {
        var resource = route.MapGroup("/api/resource").WithTags("Resources");

        resource.MapGet("module/{moduleId}", HandleGetByModule)
            .RequireAuthorization();
        resource.MapGet("{resourceId}/progress", HandleGetProgress)
            .RequireAuthorization("IsStudent");
        resource.MapPost("{resourceId}/complete", HandleComplete)
            .RequireAuthorization("IsStudent");

        resource.MapDelete("{resourceId}", HandleDelete).RequireAuthorization();
        // resource.MapPost("{moduleId}/publish/{resourceId}", HandlePublish).RequireAuthorization();
        // resource.MapPost("{moduleId}/unpublish/{resourceId}", HandleUnpublish).RequireAuthorization();
        resource.MapPost("{resourceId}/reorder", HandleReorder).RequireAuthorization();
    }

    private static async
        Task<Results<Ok<List<ResourceResponse>>, BadRequest>>
        HandleGetByModule(
            string moduleId,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var decoded = sqidsEncoder.Decode(moduleId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(
            await resourceService.GetPublishedResourcesAsync(decoded[0]));
    }

    private static async
        Task<Results<Ok<ResourceProgressResponse>, BadRequest, NotFound>>
        HandleGetProgress(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var progress = await resourceService.GetResourceProgressAsync(decoded[0]);
        return progress is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(progress);
    }

    private static async
        Task<Results<Ok<ResourceProgressResponse>, BadRequest, NotFound>>
        HandleComplete(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var progress = await resourceService.MarkResourceCompletedAsync(decoded[0]);
        return progress is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(progress);
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleDelete(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        if (!await resourceService.CheckOwnerAsync(decoded[0]))
            return TypedResults.Unauthorized();

        return await resourceService.DeleteResourceAsync(decoded[0])
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleReorder(
            string resourceId,
            [FromQuery] int orderIndex,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        if (!await resourceService.CheckOwnerAsync(decoded[0]))
            return TypedResults.Unauthorized();

        return await resourceService.ReorderResourceAsync(decoded[0], orderIndex)
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }
}
