using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
using Backend.Api.Core.Common;
using Backend.Api.Services.Courses;
using Microsoft.AspNetCore.Mvc;
using Sqids;
using Backend.Api.Core.Authorization;

namespace Backend.Api.Endpoints.Course;

public static class CourseEndpoints
{
    public static void AddCourseEndpoints(this IEndpointRouteBuilder route)
    {
        var course = route.MapGroup("/api/course").WithTags("Courses").RequireAuthorization();

        course.MapGet("{courseId}", HandleGetCourseById).WithDescription("Get by Id.");
        course.MapGet("student", HandleGetStudent).WithDescription("Get current student courses.");
        course.MapGet("instructor", HandleGetInstructor).WithDescription("Get current instructor courses.");
        course.MapGet("explore", HandleGetExplore).WithDescription("Get explore page info.");
        course.MapGet("", HandleQuery).WithDescription("Query for courses.");

        course.MapGet("{courseId}/modules", HandleGetModules);
        course.MapPost("{courseId}/modules", HandleCreateModule);

        course.MapPost("", HandleCreate);
        course.MapPut("{courseId}", HandleUpdate);
        course.MapDelete("{courseId}", HandleDelete);
        course.MapPost("{courseId}/upload-background", HandleUploadBackground);

        course.MapPost("{courseId}/publish", HandlePublish);
        course.MapPost("{courseId}/unpublish", HandleUnpublish);
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

        var result = await courseService.GetDtoAsync(decoded[0]);
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
        Task<Ok<PaginatedResponse<CourseResponse>>>
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
        Task<Results<Ok<CourseSetResponse>, NotFound, BadRequest>>
        HandleUpdate(
            string courseId,
            [FromBody] CourseSetRequest request,
            SqidsEncoder<long> sqidsEncoder,
            CourseAuthorization auth,
            CourseService courseService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound();

        var result = await courseService.UpdateCourseAsync(course, request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest>>
        HandleDelete(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            CourseAuthorization auth,
            CourseService courseService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound();

        await courseService.DeleteCourseAsync(course);
        return TypedResults.Ok();
    }

    private static async
        Task<Results<Ok<string>, BadRequest, NotFound>>
        HandleUploadBackground(
            string courseId,
            IFormFile file,
            SqidsEncoder<long> sqidsEncoder,
            CourseAuthorization auth,
            CourseService courseService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound();

        var fileId = await courseService.UploadBackgroundAsync(course, file, ct);

        return TypedResults.Ok(
            sqidsEncoder.Encode(fileId));
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest>>
        HandlePublish(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            CourseAuthorization auth,
            CourseService courseService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound();

        await courseService.SetCoursePublishStatusAsync(course, true);
        return TypedResults.Ok();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleUnpublish(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            CourseAuthorization auth,
            CourseService courseService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound();

        await courseService.SetCoursePublishStatusAsync(course, false);
        return TypedResults.Ok();
    }
}
