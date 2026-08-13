using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
using Sqids;
using Backend.Api.Services.Content;
using Backend.Api.Services.Courses;
using Backend.Api.Core.Authorization;

namespace Backend.Api.Endpoints;

public static class LessonEndpoints
{
    public static void AddLessonEndpoints(this IEndpointRouteBuilder route)
    {
        var lesson = route.MapGroup("/api/lesson");

        lesson.MapGet("{resourceId}", HandleGetById).RequireAuthorization();
        lesson.MapPost("{moduleId}", HandleCreate).RequireAuthorization();
        lesson.MapPut("{resourceId}", HandleUpdate).RequireAuthorization();

        lesson.MapPost("{resourceId}/set-publish", HandleSetPublish);
    }

    private static async
        Task<Results<Ok<LessonResponse>, BadRequest, NotFound>>
        HandleGetById(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            LessonService lessonService)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await lessonService.GetLessonByIdAsync(decoded[0]);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<LessonResponse>, BadRequest, NotFound<string>>>
        HandleCreate(
            string moduleId,
            LessonRequest request,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            LessonService lessonService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(moduleId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound("Course not found.");

        var result = await lessonService.CreateAsync(decoded[0], request, ct);
        return TypedResults.Ok(result);
    }

    private static async
        Task<Results<Ok<LessonResponse>, BadRequest, NotFound>>
        HandleUpdate(
            string resourceId,
            LessonRequest request,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            LessonService lessonService)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound();

        var result = await lessonService.CreateLessonAsync(request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<bool>, BadRequest, NotFound<string>>>
        HandleSetPublish(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            LessonService lessonService,
            CancellationToken ct,
            [AsParameters] bool isPublished = true)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound("Course not found.");

        var result = await lessonService.SetPublishStatusAsync(decoded[0], isPublished, ct);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error!.Message);
    }
}

