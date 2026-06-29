using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Core.Types;
using Backend.Core.Common;
using Backend.Services.Courses;
using Microsoft.AspNetCore.Mvc;
using Sqids;

namespace Backend.Endpoints;

public static class CourseEndpoints
{
    public static void AddCourseEndpoints(this IEndpointRouteBuilder route)
    {
        var course = route.MapGroup("/api/course").WithTags("Courses");

        course.MapGet("{courseId}", HandleGetById);
        course.MapGet("user/{userId}", HandleGetUserCourses).RequireAuthorization();
        course.MapGet("", HandleGetPublished).RequireAuthorization();
        course.MapGet("unpublished", HandleGetUnpublished).RequireAuthorization();
        course.MapGet("query", HandleQuery);
        course.MapGet("{courseId}/check", HandleCheckOwner).RequireAuthorization();

        course.MapPut("", HandleCreate).RequireAuthorization();
        course.MapPost("{courseId}", HandleUpdate).RequireAuthorization();
        course.MapDelete("{courseId}", HandleDelete).RequireAuthorization();

        course.MapPost("{courseId}/publish", HandlePublish).RequireAuthorization();
        course.MapPost("{courseId}/unpublish", HandleUnpublish).RequireAuthorization();
    }

    private static async
        Task<Results<Ok<CourseDetailResponse>, BadRequest, NotFound>>
        HandleGetById(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await courseService.GetCourseByIdAsync(decoded[0]);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<List<CourseResponse>>, BadRequest>>
        HandleGetUserCourses(
            string userId,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var decoded = sqidsEncoder.Decode(userId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(
            await courseService.GetUserCoursesAsync(decoded[0]));
    }

    private static async
        Task<Ok<List<CourseResponse>>>
        HandleGetPublished(CourseService courseService)
    {
        return TypedResults.Ok(
            await courseService.GetPublishedCoursesAsync());
    }

    private static async
        Task<Ok<List<CourseResponse>>>
        HandleGetUnpublished(CourseService courseService)
    {
        return TypedResults.Ok(
            await courseService.GetUnpublishedCoursesAsync());
    }

    private static async
        Task<Ok<QueryResponse<CourseResponse>>>
        HandleQuery([AsParameters] CourseRequest query, CourseService courseService)
    {
        return TypedResults.Ok(
            await courseService.QueryCoursesAsync(query));
    }

    private static async
        Task<Results<Ok<bool>, BadRequest>>
        HandleCheckOwner(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(
            await courseService.CheckOwnerAsync(decoded[0]));
    }

    private static async
        Task<Ok<CourseSetResponse>>
        HandleCreate([FromBody] CourseSetRequest request, CourseService courseService)
    {
        return TypedResults.Ok(
            await courseService.CreateCourseAsync(request));
    }

    private static async
        Task<Results<Ok<CourseSetResponse>, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleUpdate(
            string courseId,
            [FromBody] CourseSetRequest request,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        if (!await courseService.CheckOwnerAsync(decoded[0]))
            return TypedResults.Unauthorized();

        var result = await courseService.UpdateCourseAsync(decoded[0], request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleDelete(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        if (!await courseService.CheckOwnerAsync(decoded[0]))
            return TypedResults.Unauthorized();

        return await courseService.DeleteCourseAsync(decoded[0])
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandlePublish(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        if (!await courseService.CheckOwnerAsync(decoded[0]))
            return TypedResults.Unauthorized();

        var result = await courseService.SetCoursePublishStatusAsync(decoded[0], true);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleUnpublish(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        if (!await courseService.CheckOwnerAsync(decoded[0]))
            return TypedResults.Unauthorized();

        var result = await courseService.SetCoursePublishStatusAsync(decoded[0], false);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }
}
