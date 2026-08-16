using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
using Sqids;
using Backend.Api.Services.Courses;
using Backend.Api.Core.Authorization;
using Backend.Api.Services.Content;

namespace Backend.Api.Endpoints.Assessment;

public static class AssessmentEndpoints
{
    public static void AddAssessmentEndpoints(this IEndpointRouteBuilder route)
    {
        var assessment = route.MapGroup("/api/assessments").RequireAuthorization();

        assessment.MapGet("/", HandleQuery);
        assessment.MapGet("{resourceId}", HandleGetById);

        assessment.MapPost("{moduleId}", HandleCreate);
        assessment.MapPut("{resourceId}", HandleUpdate);

        assessment.MapPost("{resourceId}/set-publish", HandleSetPublish);

        // assessment.MapPost("{resourceId}/start", HandleStart);
        // assessment.MapPost("{resourceId}/set-answer", HandleSetAnswer);
        // assessment.MapPost("{resourceId}/submit", HandleSubmit);

        // assessment.MapPost("{resourceId}/grade/{answerId}", HandleGrade);
        // assessment.MapPut("{resourceId}/build/add-question", HandleAddQuestion);
        // assessment.MapPut("{resourceId}/build/add-question", HandleAddQuestion);

        // assessment.MapGet("{resourceId}/time", HandleGetTime);
        // assessment.MapGet("{resourceId}/restore", HandleRestore);
        // assessment.MapGet("{resourceId}/attempt/{attemptId}", HandleGetAttempt);
    }

    private static async
        Task<Results<Ok<AssessmentResponse>, BadRequest, NotFound<string>>>
        HandleGetById(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            AssessmentService assessmentService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await assessmentService.GetDtoByIdAsync(decoded[0], ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound("Assessment not found.");
    }

    private static async
        Task<Results<Ok<AssessmentResponse>, BadRequest, NotFound<string>>>
        HandleCreate(
            string moduleId,
            AssessmentRequest request,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            AssessmentService assessmentService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(moduleId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound("Course not found.");

        var result = await assessmentService.CreateAsync(decoded[0], request, ct);
        return TypedResults.Ok(result);
    }

    private static async
        Task<Results<Ok<AssessmentResponse>, BadRequest, NotFound<string>>>
        HandleUpdate(
            string resourceId,
            AssessmentRequest request,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            AssessmentService assessmentService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound("Course not found.");

        var result = await assessmentService.UpdateAsync(decoded[0], request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound("Assessment not found.");
    }


    private static async
        Task<Results<Ok<bool>, BadRequest, NotFound<string>>>
        HandleSetPublish(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            AssessmentService assessmentService,
            CancellationToken ct,
            [AsParameters] bool isPublished = true)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound("Course not found.");

        var result = await assessmentService.SetPublishStatusAsync(decoded[0], isPublished, ct);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error!.Message);
    }
}
