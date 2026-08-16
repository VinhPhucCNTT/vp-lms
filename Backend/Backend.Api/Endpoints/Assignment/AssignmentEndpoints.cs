using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
using Sqids;
using Backend.Api.Services.Content;
using Backend.Api.Services.Courses;
using Backend.Api.Core.Authorization;
using Backend.Api.Services.Submissions;
using Backend.Api.Core.Common;

namespace Backend.Api.Endpoints.Assignment;

public static class AssignmentEndpoints
{
    public static void AddAssignmentEndpoints(this IEndpointRouteBuilder route)
    {
        var assignment = route.MapGroup("/api/assignment");

        assignment.MapGet("/", HandleQuery).RequireAuthorization("IsStudent");
        assignment.MapGet("{resourceId}", HandleGetById).RequireAuthorization("IsStudent");
        assignment.MapPost("{moduleId}", HandleCreate).RequireAuthorization("IsInstructor");
        assignment.MapPut("{resourceId}", HandleUpdate).RequireAuthorization("IsInstructor");

        assignment.MapPost("{resourceId}/upload", HandleUpload)
            .RequireAuthorization("IsStudent")
            .DisableAntiforgery();
        assignment.MapPost("{resourceId}/submit", HandleSubmit).RequireAuthorization("IsStudent");
        assignment.MapDelete("{resourceId}/submit", HandleRemoveSelfSubmit).RequireAuthorization("IsStudent");

        assignment.MapGet("{resourceId}/grades/instructor", HandleGetGrades).RequireAuthorization();
        assignment.MapGet("{resourceId}/grades/", HandleGetGrades).RequireAuthorization();

        assignment.MapGet("{resourceId}/submission/student-self", HandleGetOwnSubmit).RequireAuthorization("IsStudent");
        assignment.MapGet("{resourceId}/submission/instructor-list", HandleGetSubmissions).RequireAuthorization("IsInstructor");
        assignment.MapGet("{resourceId}/submission/instructor-graded", HandleGetGraded).RequireAuthorization("IsInstructor");
        assignment.MapGet("{resourceId}/submission/instructor-ungraded", HandleGetUngraded).RequireAuthorization();

        assignment.MapPost("{resourceId}/set-publish", HandleSetPublish).RequireAuthorization("IsInstructor");
    }

    private static async Task<Ok<List<StudentAssignmentSummaryResponse>>> HandleQuery(
        AssignmentService assignmentService,
        CancellationToken ct)
    {
        return TypedResults.Ok(await assignmentService.QueryStudentAsync(ct));
    }

    private static async
        Task<Results<Ok<AssignmentResponse>, BadRequest, NotFound>>
        HandleGetById(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            AssignmentService assignmentService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await assignmentService.GetDtoByIdAsync(decoded[0], ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound();
    }

    private static async
        Task<Results<Ok<AssignmentResponse>, BadRequest, NotFound<string>>>
        HandleCreate(
            string moduleId,
            AssignmentRequest request,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            AssignmentService assignmentService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(moduleId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound("Course not found.");

        var result = await assignmentService.CreateAsync(decoded[0], request, ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound("Assignment not found.");
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

        var result = await assignmentService.UpdateAsync(decoded[0], request);
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
            SubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest("");

        var validationResult = await submissionService.ValidateFileAsync(decoded[0], file, ct);
        if (!validationResult.IsSuccess)
            return TypedResults.BadRequest($"ValidationError: {validationResult.Error!.Type}: {validationResult.Error.Message}");

        var result = await submissionService.UploadFileAsync(validationResult.Value!, file, ct);
        return TypedResults.Ok(result);
    }

        private static async
        Task<Results<Ok<SubmissionDetailResponse>, BadRequest<string>, NotFound<string>>>
        HandleSubmit(
            string resourceId,
            SubmissionRequest request,
            SqidsEncoder<long> sqidsEncoder,
            SubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest("Invalid assignment id.");

        var result = await submissionService.SubmitAsync(decoded[0], request, ct);
        if (result.IsSuccess)
            return TypedResults.Ok(result.Value!);

        return result.Error!.Code == "notfound"
            ? TypedResults.NotFound(result.Error.Message)
            : TypedResults.BadRequest(result.Error.Message);
    }

    private static async
        Task<Results<Ok, BadRequest<string>, NotFound<string>>>
        HandleRemoveSelfSubmit(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            SubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest("");

        var result = await submissionService.RemoveAsync(decoded[0], ct);
        return result
            ? TypedResults.Ok()
            : TypedResults.BadRequest("Cannot remove submission.");
    }

    private static async
        Task<Results<Ok<PaginatedResponse<AssignmentGradeResponse>>, BadRequest, NotFound<string>>>
        HandleGetGrades(
            string resourceId,
            [AsParameters] PageRequest page,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            AssignmentGradeService assignmentGradeService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound("Course not found.");

        var result = await assignmentGradeService.GetAssignmentGradesAsync(decoded[0], page, ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound("Assignment not found.");
    }

    private static async
        Task<Results<Ok<SubmissionDetailResponse>, BadRequest, NotFound<string>>>
        HandleGetOwnSubmit(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            SubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await submissionService.GetOwnDetailAsync(decoded[0], ct);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error!.Message);
    }

    private static async
        Task<Results<Ok<PaginatedResponse<SubmissionResponse>>, BadRequest, NotFound<string>>>
        HandleGetSubmissions(
            string resourceId,
            [AsParameters] PageRequest page,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            SubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound("Course not found.");

        var result = await submissionService.GetListAsync(decoded[0], page, ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound("Assignment not found.");
    }

    private static async
        Task<Results<Ok<PaginatedResponse<SubmissionResponse>>, BadRequest, NotFound<string>>>
        HandleGetGraded(
            string resourceId,
            [AsParameters] PageRequest page,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            SubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound("Course not found.");

        var result = await submissionService.GetGradedAsync(decoded[0], page, ct);
        return TypedResults.Ok(result);
    }

    private static async
        Task<Results<Ok<PaginatedResponse<SubmissionResponse>>, BadRequest, NotFound<string>>>
        HandleGetUngraded(
            string resourceId,
            [AsParameters] PageRequest page,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            SubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound("Course not found.");

        var result = await submissionService.GetUngradedAsync(decoded[0], page, ct);
        return TypedResults.Ok(result);
    }


    private static async
        Task<Results<Ok<bool>, BadRequest, NotFound<string>>>
        HandleSetPublish(
            string resourceId,
            SqidsEncoder<long> sqidsEncoder,
            CourseService courseService,
            CourseAuthorization auth,
            AssignmentService assignmentService,
            CancellationToken ct,
            [AsParameters] bool isPublished = true)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var course = await courseService.GetFromModuleAsync(decoded[0]);
        if (course is null || !await auth.IsCourseOwnerAsync(course))
            return TypedResults.NotFound("Course not found.");

        var result = await assignmentService.SetPublishStatusAsync(decoded[0], isPublished, ct);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error!.Message);
    }
}
