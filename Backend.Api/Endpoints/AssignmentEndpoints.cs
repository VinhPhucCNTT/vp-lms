using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
using Sqids;
using Backend.Api.Services.Content;
using Backend.Api.Services.Courses;
using Backend.Api.Core.Authorization;
using System.ComponentModel.DataAnnotations;

namespace Backend.Api.Endpoints;

public static class AssignmentEndpoints
{
    public static void AddAssignmentEndpoints(this IEndpointRouteBuilder route)
    {
        var assignment = route.MapGroup("/api/assignment").RequireAuthorization();

        assignment.MapGet("{resourceId}", HandleGetById);
        assignment.MapPost("{moduleId}", HandleCreate);
        assignment.MapPut("{resourceId}", HandleUpdate);

        assignment.MapPost("{resourceId}/upload", HandleUpload);
        assignment.MapPost("{resourceId}/submit", HandleSubmit);
        // assignment.MapGet("{resourceId}/file", HandleGetFile);
        //
        // assignment.MapPost("{submissionId}/grade", HandleGrade);
    }

    private static async
        Task<Results<Ok<AssignmentResponse>, BadRequest, NotFound>>
        HandleGetById(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            AssignmentService assignmentService)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await assignmentService.GetAssignmentByIdAsync(decoded[0]);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<AssignmentResponse>, BadRequest, NotFound>>
        HandleCreate(
            string moduleId,
            AssignmentRequest request,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            AssignmentService assignmentService)
    {
        var decoded = sqidsEncoder.Decode(moduleId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound();

        var result = await assignmentService.CreateAssignmentAsync(request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<AssignmentResponse>, BadRequest, NotFound>>
        HandleUpdate(
            string resourceId,
            AssignmentRequest request,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            AssignmentService assignmentService)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound();

        var result = await assignmentService.UpdateAssignmentAsync(decoded[0], request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<AssignmentFileResponse>, BadRequest<string>, NotFound>>
        HandleUpload(
            string resourceId,
            IFormFile file,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            AssignmentService assignmentService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest("");

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsParticipantAsync(course.Id))
            return TypedResults.NotFound();

        var validationResult = await assignmentService.ValidateSubmittedFileAsync(decoded[0], file, ct);
        if (!validationResult.IsSuccess)
            return TypedResults.BadRequest($"ValidationError: {validationResult.Error!.Type}: {validationResult.Error.Message}");

        var result = await assignmentService.UploadAssignmentFileAsync(validationResult.Value!, file, ct);
        return TypedResults.Ok(result);
    }

    private static async
        Task<Results<Ok<SubmissionResponse>, BadRequest, NotFound>>
        HandleSubmit(
            string resourceId,
            SubmissionRequest request,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            AssignmentService assignmentService)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsParticipantAsync(course.Id))
            return TypedResults.NotFound();

        var result = await assignmentService.SubmitAssignmentAsync(decoded[0], request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }
}
