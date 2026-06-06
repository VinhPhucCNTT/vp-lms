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

        course.MapGet("{courseSqid}", HandleGetById);
        course.MapGet("user/{userSqid}", HandleGetUserCourses);
        course.MapGet("", HandleGetPublished);
        course.MapGet("unpublished", HandleGetUnpublished).RequireAuthorization();
        course.MapGet("query", HandleQuery);
        course.MapGet("{courseSqid}/check", HandleCheckOwner).RequireAuthorization();

        course.MapPut("", HandleCreate).RequireAuthorization();
        course.MapPost("{courseSqid}", HandleUpdate).RequireAuthorization();
        course.MapDelete("{courseSqid}", HandleDelete).RequireAuthorization();

        course.MapPost("{courseSqid}/publish", HandlePublish).RequireAuthorization();
        course.MapPost("{courseSqid}/unpublish", HandleUnpublish).RequireAuthorization();
    }

    private static async
        Task<Results<Ok<CourseDetailResponse>, NotFound>>
        HandleGetById(
            [FromRoute] string courseSqid,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var courseId = sqidsEncoder.Decode(courseSqid).Single();
        var result = await courseService.GetCourseByIdAsync(courseId);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Ok<List<CourseResponse>>>
        HandleGetUserCourses(
            [FromRoute] string userSqid,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var userId = sqidsEncoder.Decode(userSqid).Single();
        return TypedResults.Ok(
            await courseService.GetUserCoursesAsync(userId));
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
        Task<Ok<CourseSetResponse>>
        HandleCreate([FromBody] CourseSetRequest request, CourseService courseService)
    {
        return TypedResults.Ok(
            await courseService.CreateCourseAsync(request));
    }

    private static async
        Task<Results<Ok<CourseSetResponse>, NotFound, UnauthorizedHttpResult>>
        HandleUpdate(
            [FromRoute] string courseSqid,
            [FromBody] CourseSetRequest request,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var courseId = sqidsEncoder.Decode(courseSqid).Single();
        if (!await courseService.CheckOwnerAsync(courseId))
            return TypedResults.Unauthorized();

        var result = await courseService.UpdateCourseAsync(courseId, request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, UnauthorizedHttpResult>>
        HandleDelete(
            [FromRoute] string courseSqid,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var courseId = sqidsEncoder.Decode(courseSqid).Single();
        if (!await courseService.CheckOwnerAsync(courseId))
            return TypedResults.Unauthorized();

        var result = await courseService.DeleteCourseAsync(courseId);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, UnauthorizedHttpResult>>
        HandlePublish(
            [FromRoute] string courseSqid,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var courseId = sqidsEncoder.Decode(courseSqid).Single();
        if (!await courseService.CheckOwnerAsync(courseId))
            return TypedResults.Unauthorized();

        var result = await courseService.SetCoursePublishStatusAsync(courseId, true);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, UnauthorizedHttpResult>>
        HandleUnpublish(
            [FromRoute] string courseSqid,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var courseId = sqidsEncoder.Decode(courseSqid).Single();
        if (!await courseService.CheckOwnerAsync(courseId))
            return TypedResults.Unauthorized();

        var result = await courseService.SetCoursePublishStatusAsync(courseId, false);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Ok<bool>>
        HandleCheckOwner(
            [FromRoute] string courseSqid,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var courseId = sqidsEncoder.Decode(courseSqid).Single();
        return TypedResults.Ok(
            await courseService.CheckOwnerAsync(courseId));
    }
}
