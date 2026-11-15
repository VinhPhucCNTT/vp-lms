using Backend.Api.Core.Authorization;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Content;
using Backend.Api.Services.Courses;
using Backend.Api.Services.Submissions;
using Microsoft.AspNetCore.Http.HttpResults;
using Sqids;

namespace Backend.Api.Endpoints.Assignment;

public static class AssignmentEndpoints
{
    public static void AddAssignmentEndpoints(this IEndpointRouteBuilder route)
    {
        var assignment = route.MapGroup("/api/assignment");

        assignment.MapGet("/", HandleQuery).RequireAuthorization("IsStudent");
        assignment.MapGet("{activityId}", HandleGetById).RequireAuthorization("IsStudent");
        assignment.MapPost("{moduleId}", HandleCreate).RequireAuthorization("IsInstructor");
        assignment.MapPut("{activityId}", HandleUpdate).RequireAuthorization("IsInstructor");

        assignment.MapPost("{activityId}/upload", HandleUpload)
            .RequireAuthorization("IsStudent")
            .DisableAntiforgery();
        assignment.MapPost("{activityId}/submit", HandleSubmit).RequireAuthorization("IsStudent");
        assignment.MapDelete("{activityId}/submit", HandleRemoveSelfSubmit).RequireAuthorization("IsStudent");

        assignment.MapGet("{activityId}/grades/instructor", HandleGetGrades).RequireAuthorization();
        assignment.MapGet("{activityId}/grades/", HandleGetGrades).RequireAuthorization();

        assignment.MapGet("{activityId}/submission/student-self", HandleGetOwnSubmit).RequireAuthorization("IsStudent");
        assignment.MapGet("{activityId}/submission/instructor-list", HandleGetSubmissions).RequireAuthorization("IsInstructor");
        assignment.MapGet("{activityId}/submission/instructor-graded", HandleGetGraded).RequireAuthorization("IsInstructor");
        assignment.MapGet("{activityId}/submission/instructor-ungraded", HandleGetUngraded).RequireAuthorization();

        assignment.MapPost("{activityId}/set-publish", HandleSetPublish).RequireAuthorization("IsInstructor");
    }

    private static async Task<Ok<List<StudentAssignmentSummaryResponse>>> HandleQuery(
        IAssignmentService assignmentService,
        CancellationToken ct)
    {
        return TypedResults.Ok(await assignmentService.QueryStudentAsync(ct));
    }

    private static async
        Task<Results<Ok<AssignmentResponse>, BadRequest, NotFound>>
        HandleGetById(
            string activityId,
            SqidsEncoder<long> sqidsEncoder,
            IAssignmentService assignmentService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);
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
            ICourseService courseService,
            ICourseAuthorization auth,
            IAssignmentService assignmentService,
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
            string activityId,
            AssignmentRequest request,
            SqidsEncoder<long> sqidsEncoder,
            ICourseService courseService,
            ICourseAuthorization auth,
            IAssignmentService assignmentService)
    {
        var decoded = sqidsEncoder.Decode(activityId);
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
        Task<Results<Ok<AssignmentFileResponse>, BadRequest<string>, NotFound, InternalServerError<string>>>
        HandleUpload(
            string activityId,
            IFormFile file,
            SqidsEncoder<long> sqidsEncoder,
            ISubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest("");

        var validationResult = await submissionService.ValidateFileAsync(decoded[0], file, ct);
        if (!validationResult.IsSuccess)
            return TypedResults.BadRequest($"ValidationError: {validationResult.Error!.Type}: {validationResult.Error.Message}");

        var result = await submissionService.UploadFileAsync(validationResult.Value!, file, ct);
        if (!result.IsSuccess)
            return TypedResults.InternalServerError(result.Error!.Code);

        return TypedResults.Ok(result.Value);
    }

        private static async
        Task<Results<Ok<SubmissionDetailResponse>, BadRequest<string>, NotFound<string>>>
        HandleSubmit(
            string activityId,
            SubmissionRequest request,
            SqidsEncoder<long> sqidsEncoder,
            ISubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);
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
            string activityId,
            SqidsEncoder<long> sqidsEncoder,
            ISubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);
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
            string activityId,
            [AsParameters] PageRequest page,
            SqidsEncoder<long> sqidsEncoder,
            ICourseService courseService,
            ICourseAuthorization auth,
            IAssignmentGradeService assignmentGradeService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);
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
            string activityId,
            SqidsEncoder<long> sqidsEncoder,
            ISubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);
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
            string activityId,
            [AsParameters] PageRequest page,
            SqidsEncoder<long> sqidsEncoder,
            ICourseService courseService,
            ICourseAuthorization auth,
            ISubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);
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
            string activityId,
            [AsParameters] PageRequest page,
            SqidsEncoder<long> sqidsEncoder,
            ICourseService courseService,
            ICourseAuthorization auth,
            ISubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);
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
            string activityId,
            [AsParameters] PageRequest page,
            SqidsEncoder<long> sqidsEncoder,
            ICourseService courseService,
            ICourseAuthorization auth,
            ISubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);
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
            string activityId,
            SqidsEncoder<long> sqidsEncoder,
            ICourseService courseService,
            ICourseAuthorization auth,
            IAssignmentService assignmentService,
            CancellationToken ct,
            [AsParameters] bool isPublished = true)
    {
        var decoded = sqidsEncoder.Decode(activityId);
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
