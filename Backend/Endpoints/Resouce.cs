using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Core.Types;
using Backend.Services.Courses;
using Microsoft.AspNetCore.Mvc;
using Sqids;

namespace Backend.Endpoints;

public static class ResourceEndpoints
{
    public static void AddResourceEndpoints(this IEndpointRouteBuilder route)
    {
        var resource = route.MapGroup("/api/resource").WithTags("Resources");

        resource.MapGet("{resourceSqid}", HandleGetById);
        resource.MapGet("{moduleSqid}/published", HandleGetPublished);
        resource.MapGet("{moduleSqid}/unpublished", HandleGetUnpublished);
        resource.MapGet("{resourceSqid}/check", HandleCheckOwner);

        resource.MapPut("{moduleSqid}", HandleCreate);
        resource.MapPost("{resourceSqid}", HandleUpdate);
        resource.MapDelete("{resourceSqid}", HandleDelete);

        resource.MapPost("{moduleSqid}/publish/{resourceSqid}", HandlePublish);
        resource.MapPost("{moduleSqid}/unpublish/{resourceSqid}", HandleUnpublish);
        resource.MapPost("{moduleSqid}/bulk-publish", HandleBulkPublish);
        resource.MapPost("{moduleSqid}/bulk-unpublish", HandleBulkUnpublish);
        resource.MapPost("{resourceSqid}/reorder", HandleReorder);
    }

    private static async
        Task<Results<Ok<ResourceDetailResponse>, NotFound>>
        HandleGetById(
            [FromRoute] string resourceSqid,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var resourceId = sqidsEncoder.Decode(resourceSqid).Single();
        var result = await resourceService.GetResourceByIdAsync(resourceId);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Ok<List<ResourceResponse>>>
        HandleGetPublished(
            [FromRoute] string moduleSqid,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();
        return TypedResults.Ok(
            await resourceService.GetPublishedResourcesAsync(moduleId));
    }

    private static async
        Task<Ok<List<ResourceResponse>>>
        HandleGetUnpublished(
            [FromRoute] string moduleSqid,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();
        return TypedResults.Ok(
            await resourceService.GetUnpublishedResourcesAsync(moduleId));
    }

    private static async
        Task<Ok<bool>>
        HandleCheckOwner(
            [FromRoute] string resourceSqid,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var resourceId = sqidsEncoder.Decode(resourceSqid).Single();
        return TypedResults.Ok(
            await resourceService.CheckOwnerAsync(resourceId));
    }

    private static async
        Task<Ok<ResourceSetResponse>>
        HandleCreate(
            [FromRoute] string moduleSqid,
            [FromBody] ResourceCreateRequest request,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();
        return TypedResults.Ok(
            await resourceService.CreateResourceAsync(moduleId, request));
    }

    private static async
        Task<Results<Ok<ResourceSetResponse>, NotFound, UnauthorizedHttpResult>>
        HandleUpdate(
            [FromRoute] string resourceSqid,
            [FromBody] ResourceUpdateRequest request,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var resourceId = sqidsEncoder.Decode(resourceSqid).Single();
        if (!await resourceService.CheckOwnerAsync(resourceId))
            return TypedResults.Unauthorized();

        var result = await resourceService.UpdateResourceAsync(resourceId, request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, UnauthorizedHttpResult>>
        HandleDelete(
            [FromRoute] string resourceSqid,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var resourceId = sqidsEncoder.Decode(resourceSqid).Single();
        if (!await resourceService.CheckOwnerAsync(resourceId))
            return TypedResults.Unauthorized();

        return await resourceService.DeleteResourceAsync(resourceId)
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, UnauthorizedHttpResult>>
        HandlePublish(
            [FromRoute] string moduleSqid,
            [FromRoute] string resourceSqid,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var resourceId = sqidsEncoder.Decode(resourceSqid).Single();
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();
        if (!await resourceService.CheckOwnerAsync(resourceId))
            return TypedResults.Unauthorized();

        var result = await resourceService.SetResourcePublishStatusAsync(moduleId, resourceId, true);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, UnauthorizedHttpResult>>
        HandleUnpublish(
            [FromRoute] string moduleSqid,
            [FromRoute] string resourceSqid,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var resourceId = sqidsEncoder.Decode(resourceSqid).Single();
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();
        if (!await resourceService.CheckOwnerAsync(resourceId))
            return TypedResults.Unauthorized();

        var result = await resourceService.SetResourcePublishStatusAsync(moduleId, resourceId, false);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<int>, NotFound, UnauthorizedHttpResult>>
        HandleBulkPublish(
            [FromRoute] string moduleSqid,
            [FromBody] List<string> resourceSqids,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        if (resourceSqids is null || resourceSqids.Count == 0)
            return TypedResults.Ok(0);
        var resourceIds = resourceSqids
            .Select(sqid => sqidsEncoder.Decode(sqid).SingleOrDefault())
            .ToList();
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();

        var count = await resourceService.SetResourcesPublishStatusAsync(moduleId, resourceIds, true);
        return count > 0
            ? TypedResults.Ok(count)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<int>, NotFound, UnauthorizedHttpResult>>
        HandleBulkUnpublish(
            [FromRoute] string moduleSqid,
            [FromBody] List<string> resourceSqids,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        if (resourceSqids is null || resourceSqids.Count == 0)
            return TypedResults.Ok(0);
        var resourceIds = resourceSqids
            .Select(sqid => sqidsEncoder.Decode(sqid).SingleOrDefault())
            .ToList();
        var moduleId = sqidsEncoder.Decode(moduleSqid).Single();

        var count = await resourceService.SetResourcesPublishStatusAsync(moduleId, resourceIds, false);
        return count > 0
            ? TypedResults.Ok(count)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, UnauthorizedHttpResult>>
        HandleReorder(
            [FromRoute] string resourceSqid,
            [FromQuery] int orderIndex,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var resourceId = sqidsEncoder.Decode(resourceSqid).Single();
        if (!await resourceService.CheckOwnerAsync(resourceId))
            return TypedResults.Unauthorized();

        return await resourceService.ReorderResourceAsync(resourceId, orderIndex)
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }
}
