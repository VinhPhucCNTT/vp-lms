using Backend.Api.Core.Types;
using Backend.Api.Services.Courses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Sqids;

namespace Backend.Api.Endpoints.Course;

public static class ActivityEndpoints
{
    public static void AddActivityEndpoints(this IEndpointRouteBuilder route)
    {
        var activity = route.MapGroup("/api/activity").WithTags("Activities");

        activity.MapGet("{activityId}/progress", HandleGetProgress)
            .RequireAuthorization("IsStudent");
        activity.MapPost("{activityId}/complete", HandleComplete)
            .RequireAuthorization("IsStudent");

        activity.MapDelete("{activityId}", HandleDelete).RequireAuthorization();
        // activity.MapPost("{moduleId}/publish/{activityId}", HandlePublish).RequireAuthorization();
        // activity.MapPost("{moduleId}/unpublish/{activityId}", HandleUnpublish).RequireAuthorization();
        activity.MapPost("{activityId}/reorder", HandleReorder).RequireAuthorization();
    }

    private static async
        Task<Results<Ok<ActivityProgressResponse>, BadRequest, NotFound>>
        HandleGetProgress(
            string activityId,
            SqidsEncoder<long> sqidsEncoder,
            IActivityService activityService)
    {
        var decoded = sqidsEncoder.Decode(activityId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var progress = await activityService.GetActivityProgressAsync(decoded[0]);
        return progress is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(progress);
    }

    private static async
        Task<Results<Ok<ActivityProgressResponse>, BadRequest, NotFound>>
        HandleComplete(
            string activityId,
            SqidsEncoder<long> sqidsEncoder,
            IActivityService activityService)
    {
        var decoded = sqidsEncoder.Decode(activityId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var progress = await activityService.MarkActivityCompletedAsync(decoded[0]);
        return progress is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(progress);
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleDelete(
            string activityId,
            SqidsEncoder<long> sqidsEncoder,
            IActivityService activityService)
    {
        var decoded = sqidsEncoder.Decode(activityId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        if (!await activityService.CheckOwnerAsync(decoded[0]))
            return TypedResults.Unauthorized();

        return await activityService.DeleteActivityAsync(decoded[0])
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleReorder(
            string activityId,
            [FromQuery] int orderIndex,
            SqidsEncoder<long> sqidsEncoder,
            IActivityService activityService)
    {
        var decoded = sqidsEncoder.Decode(activityId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        if (!await activityService.CheckOwnerAsync(decoded[0]))
            return TypedResults.Unauthorized();

        return await activityService.ReorderActivityAsync(decoded[0], orderIndex)
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }
}
