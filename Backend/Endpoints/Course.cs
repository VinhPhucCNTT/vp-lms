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
        var course = route.MapGroup("/api/course");

        course.MapGet("{courseSqid}", HandleGetById);
        course.MapGet("query", HandleQuery);
        course.MapPut("", HandleCreate).RequireAuthorization();
        course.MapPost("{courseSqid}", HandleUpdate).RequireAuthorization();
        course.MapDelete("{courseSqid}", HandleDelete).RequireAuthorization();
        course.MapPost("{courseSqid}/set", HandleSetPublish).RequireAuthorization();
        course.MapGet("{courseSqid}/check", HandleCheckValid).RequireAuthorization();
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
        Task<Ok<QueryResponse<CourseResponse>>>
        HandleQuery([AsParameters] CourseRequest query, CourseService courseService)
    {
        var result = await courseService.QueryCoursesAsync(query);
        return TypedResults.Ok(result);
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
        if (!await courseService.IsUserValidAsync(courseId))
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
        if (!await courseService.IsUserValidAsync(courseId))
            return TypedResults.Unauthorized();

        var result = await courseService.DeleteCourseAsync(courseId);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, UnauthorizedHttpResult>>
        HandleSetPublish(
            [FromRoute] string courseSqid,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            [FromQuery] bool isPublished = true)
    {
        var courseId = sqidsEncoder.Decode(courseSqid).Single();
        if (!await courseService.IsUserValidAsync(courseId))
            return TypedResults.Unauthorized();

        var result = await courseService.SetCoursePublishStatusAsync(courseId, isPublished);
        return result
            ? TypedResults.Ok()
            : TypedResults.NotFound();
    }

    private static async
        Task<Ok<bool>>
        HandleCheckValid(
            [FromRoute] string courseSqid,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService)
    {
        var courseId = sqidsEncoder.Decode(courseSqid).Single();
        return TypedResults.Ok(
            await courseService.IsUserValidAsync(courseId));
    }
}
