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

        resource.MapGet("{resourceId}", HandleGetById);
        resource.MapGet("{moduleId}/published", HandleGetPublished).RequireAuthorization();
        resource.MapGet("{moduleId}/unpublished", HandleGetUnpublished).RequireAuthorization();
        resource.MapGet("{resourceId}/check", HandleCheckOwner).RequireAuthorization();

        resource.MapPut("{moduleId}", HandleCreate).RequireAuthorization();
        resource.MapPost("{resourceId}", HandleUpdate).RequireAuthorization();
        resource.MapDelete("{resourceId}", HandleDelete).RequireAuthorization();

        resource.MapPost("{moduleId}/publish/{resourceId}", HandlePublish).RequireAuthorization();
        resource.MapPost("{moduleId}/unpublish/{resourceId}", HandleUnpublish).RequireAuthorization();
        resource.MapPost("{moduleId}/bulk-publish", HandleBulkPublish).RequireAuthorization();
        resource.MapPost("{moduleId}/bulk-unpublish", HandleBulkUnpublish).RequireAuthorization();
        resource.MapPost("{resourceId}/reorder", HandleReorder).RequireAuthorization();
    }

    private static async
        Task<Results<Ok<ResourceDetailResponse>, BadRequest, NotFound>>
        HandleGetById(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await resourceService.GetResourceByIdAsync(decoded[0]);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<List<ResourceResponse>>, BadRequest>>
        HandleGetPublished(
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
        Task<Results<Ok<List<ResourceResponse>>, BadRequest>>
        HandleGetUnpublished(
            string moduleId,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var decoded = sqidsEncoder.Decode(moduleId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(
            await resourceService.GetUnpublishedResourcesAsync(decoded[0]));
    }

    private static async
        Task<Results<Ok<bool>, BadRequest>>
        HandleCheckOwner(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(
            await resourceService.CheckOwnerAsync(decoded[0]));
    }

    private static async
        Task<Results<Ok<ResourceSetResponse>, BadRequest>>
        HandleCreate(
            string moduleId,
            [FromBody] ResourceCreateRequest request,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var decoded = sqidsEncoder.Decode(moduleId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(
            await resourceService.CreateResourceAsync(decoded[0], request));
    }

    private static async
        Task<Results<Ok<ResourceSetResponse>, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleUpdate(
            string resourceId,
            [FromBody] ResourceUpdateRequest request,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        if (!await resourceService.CheckOwnerAsync(decoded[0]))
            return TypedResults.Unauthorized();

        var result = await resourceService.UpdateResourceAsync(decoded[0], request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
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
        HandlePublish(
            string moduleId,
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var dResourceId = sqidsEncoder.Decode(resourceId);
        var dModuleId = sqidsEncoder.Decode(moduleId);
        if (dResourceId.Count != 1 || dModuleId.Count != 1)
            return TypedResults.BadRequest();

        if (!await resourceService.CheckOwnerAsync(dResourceId[0]))
            return TypedResults.Unauthorized();

        var result = await resourceService.SetResourcePublishStatusAsync(dModuleId[0], dResourceId[0], true);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleUnpublish(
            string moduleId,
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        var dResourceId = sqidsEncoder.Decode(resourceId);
        var dModuleId = sqidsEncoder.Decode(moduleId);
        if (dResourceId.Count != 1 || dModuleId.Count != 1)
            return TypedResults.BadRequest();

        if (!await resourceService.CheckOwnerAsync(dResourceId[0]))
            return TypedResults.Unauthorized();

        var result = await resourceService.SetResourcePublishStatusAsync(dModuleId[0], dResourceId[0], false);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<int>, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleBulkPublish(
            string moduleId,
            [FromBody] List<string> resourceIds,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        if (resourceIds is null || resourceIds.Count == 0)
            return TypedResults.Ok(0);

        var dModuleId = sqidsEncoder.Decode(moduleId);
        if (dModuleId.Count != 1)
            return TypedResults.BadRequest();

        List<long> dResourceIds = [];
        foreach (var id in resourceIds)
        {
            var decoded = sqidsEncoder.Decode(id);
            if (decoded.Count != 1)
                return TypedResults.BadRequest();
            dResourceIds.Add(decoded[0]);
        }

        var count = await resourceService.SetResourcesPublishStatusAsync(dModuleId[0], dResourceIds, true);
        return count > 0
            ? TypedResults.Ok(count)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<int>, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleBulkUnpublish(
            string moduleId,
            [FromBody] List<string> resourceIds,
            SqidsEncoder<long> sqidsEncoder,
            ResourceService resourceService)
    {
        if (resourceIds is null || resourceIds.Count == 0)
            return TypedResults.Ok(0);

        var dModuleId = sqidsEncoder.Decode(moduleId);
        if (dModuleId.Count != 1)
            return TypedResults.BadRequest();

        List<long> dResourceIds = [];
        foreach (var id in resourceIds)
        {
            var decoded = sqidsEncoder.Decode(id);
            if (decoded.Count != 1)
                return TypedResults.BadRequest();
            dResourceIds.Add(decoded[0]);
        }


        var count = await resourceService.SetResourcesPublishStatusAsync(dModuleId[0], dResourceIds, false);
        return count > 0
            ? TypedResults.Ok(count)
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
