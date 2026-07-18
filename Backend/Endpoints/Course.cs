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

        course.MapGet("{courseId}", HandleGetCourseById).RequireAuthorization();
        course.MapGet("student", HandleGetStudent).RequireAuthorization();
        course.MapGet("instructor", HandleGetInstructor).RequireAuthorization();
        course.MapGet("explore", HandleGetExplore).RequireAuthorization();
        course.MapGet("", HandleQuery).RequireAuthorization();

        course.MapGet("{courseId}/modules", HandleGetModules).RequireAuthorization();
        course.MapPost("{courseId}/modules", HandleCreateModule).RequireAuthorization();

        course.MapPost("", HandleCreate).RequireAuthorization();
        course.MapPut("{courseId}", HandleUpdate).RequireAuthorization();
        course.MapDelete("{courseId}", HandleDelete).RequireAuthorization();

        course.MapPost("{courseId}/publish", HandlePublish).RequireAuthorization();
        course.MapPost("{courseId}/unpublish", HandleUnpublish).RequireAuthorization();
    }

    private static async
        Task<Results<Ok<CourseResponse>, BadRequest, NotFound>>
        HandleGetCourseById(
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
        Task<Ok<List<CourseStudentResponse>>>
        HandleGetStudent(CourseService courseService)
    {
        return TypedResults.Ok(
            await courseService.GetStudentCoursesAsync());
    }

    private static async
        Task<Ok<List<CourseResponse>>>
        HandleGetInstructor(CourseService courseService)
    {
        return TypedResults.Ok(
            await courseService.GetInstructorCoursesAsync());
    }

    private static async
        Task<Ok<CourseExploreResponse>>
        HandleGetExplore(CourseService courseService)
    {
        return TypedResults.Ok(
            await courseService.GetExploreAsync());
    } 

    private static async
        Task<Ok<QueryResponse<CourseResponse>>>
        HandleQuery([AsParameters] CourseRequest query, CourseService courseService)
    {
        return TypedResults.Ok(
            await courseService.QueryCoursesAsync(query));
    }

    private static async
        Task<Results<Ok<List<ModuleResponse>>, BadRequest>>
        HandleGetModules(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            ModuleService moduleService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(
            await moduleService.GetCourseModulesAsync(decoded[0]));
    }

    private static async
        Task<Results<Ok<ModuleSetResponse>, BadRequest>>
        HandleCreateModule(
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
