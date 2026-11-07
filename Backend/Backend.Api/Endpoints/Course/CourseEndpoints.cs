using Backend.Api.Core.Authorization;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Courses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Sqids;

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
        course.MapGet("recent", HandleGetRecent).WithDescription("Get recently accessed courses.");

        course.MapGet("{courseId}/modules", HandleGetModules);
        course.MapPost("{courseId}/modules", HandleCreateModule);

        course.MapPost("", HandleCreate).RequireAuthorization("IsInstructor");
        course.MapPut("{courseId}", HandleUpdate);
        course.MapDelete("{courseId}", HandleDelete);

        course.MapPost("{courseId}/publish", HandlePublish);
        course.MapPost("{courseId}/unpublish", HandleUnpublish);
    }

    private static async
        Task<Results<Ok<CourseResponse>, BadRequest, NotFound>>
        HandleGetCourseById(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            ICourseService courseService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await courseService.GetDtoAsync(decoded[0], ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Ok<List<CourseStudentResponse>>>
        HandleGetStudent(ICourseService courseService,
            CancellationToken ct) => TypedResults.Ok(
            await courseService.GetStudentCoursesAsync(ct));

    private static async
        Task<Ok<List<CourseResponse>>>
        HandleGetInstructor(ICourseService courseService,
            CancellationToken ct) => TypedResults.Ok(
            await courseService.GetInstructorCoursesAsync(ct));

    private static async
        Task<Ok<CourseExploreResponse>>
        HandleGetExplore(ICourseService courseService,
            CancellationToken ct) => TypedResults.Ok(
            await courseService.GetExploreAsync(ct));

    private static async
        Task<Ok<PaginatedResponse<CourseResponse>>>
        HandleQuery([AsParameters] CourseRequest query, ICourseService courseService,
            CancellationToken ct) => TypedResults.Ok(
            await courseService.QueryCoursesAsync(query, ct));

    private static async
        Task<Results<Ok<List<ModuleResponse>>, BadRequest>>
        HandleGetModules(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            IModuleService moduleService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(
            await moduleService.GetCourseModulesAsync(decoded[0], ct));
    }

    private static async
        Task<Results<Ok<ModuleSetResponse>, BadRequest>>
        HandleCreateModule(
            string courseId,
            [FromBody] ModuleSetRequest request,
            SqidsEncoder<long> sqidsEncoder,
            IModuleService moduleService)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(
            await moduleService.CreateModuleAsync(decoded[0], request));
    }

    private static async
        Task<Ok<CourseSetResponse>>
        HandleCreate([FromBody] CourseSetRequest request, ICourseService courseService,
            CancellationToken ct) => TypedResults.Ok(
            await courseService.CreateCourseAsync(request, ct));

    private static async
        Task<Results<Ok<CourseSetResponse>, NotFound, BadRequest>>
        HandleUpdate(
            string courseId,
            [FromBody] CourseSetRequest request,
            IFormFile file,
            SqidsEncoder<long> sqidsEncoder,
            ICourseAuthorization auth,
            ICourseService courseService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetAsync(decoded[0], ct);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound();

        var result = await courseService.UpdateCourseAsync(course, request, file, ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest>>
        HandleDelete(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            ICourseAuthorization auth,
            ICourseService courseService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetAsync(decoded[0], ct);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound();

        await courseService.DeleteCourseAsync(course, ct);
        return TypedResults.Ok();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest>>
        HandlePublish(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            ICourseAuthorization auth,
            ICourseService courseService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetAsync(decoded[0], ct);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound();

        await courseService.SetCoursePublishStatusAsync(course, true, ct);
        return TypedResults.Ok();
    }

    private static async
        Task<Results<Ok, NotFound, BadRequest, UnauthorizedHttpResult>>
        HandleUnpublish(
            string courseId,
            SqidsEncoder<long> sqidsEncoder,
            ICourseAuthorization auth,
            ICourseService courseService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(courseId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetAsync(decoded[0], ct);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound();

        await courseService.SetCoursePublishStatusAsync(course, false, ct);
        return TypedResults.Ok();
    }

    private static async
        Task<IResult>
        HandleGetRecent(
            ICourseService courseService,
            CancellationToken ct,
            [AsParameters] int count = 10)
    {
        var result = await courseService.GetRecentAsync(count, ct);
        return TypedResults.Ok(result);
    }
}
