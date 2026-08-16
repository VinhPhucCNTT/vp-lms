using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
using Sqids;
using Backend.Api.Services.Courses;
using Backend.Api.Core.Authorization;
using Backend.Api.Services.Submissions;
using Backend.Api.Core.Common;

namespace Backend.Api.Endpoints;

public static class SubmissionEndpoints
{
    public static void AddSubmissionEndpoints(this IEndpointRouteBuilder route)
    {
        var submission = route.MapGroup("/api/assignment-submission");

        submission.MapGet("{submissionId}", HandleGetById).RequireAuthorization("IsInstructor");
        submission.MapGet("/student-self-grades", HandleGetOwnGrades).RequireAuthorization("IsStudent");

        submission.MapGet("{submissionId}/grade", HandleGetGrade).RequireAuthorization();
        submission.MapPost("{submissionId}/grade", HandleAddGrade).RequireAuthorization("IsInstructor");
        submission.MapPut("{submissionId}/grade", HandleUpdateGrade).RequireAuthorization("IsInstructor");
        submission.MapDelete("{submissionId}/grade", HandleRemoveGrade).RequireAuthorization("IsStudent");
    }

    private static async
        Task<Results<Ok<SubmissionDetailResponse>, BadRequest, NotFound<string>>>
        HandleGetById(
            string submissionId,
            SqidsEncoder<long> sqidsEncoder,
            SubmissionService submissionService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(submissionId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await submissionService.GetOwnDetailAsync(decoded[0], ct);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error!.Message);
    }

    private static async
        Task<Results<Ok<AssignmentGradeResponse>, BadRequest, NotFound<string>>>
        HandleGetGrade(
            string submissionId,
            SqidsEncoder<long> sqidsEncoder,
            AssignmentGradeService assignmentGradeService,
            CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(submissionId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await assignmentGradeService.GetGradeAsync(decoded[0], ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound("Submission not found.");
    }

    private static async
        Task<Results<Ok<SubmissionDetailResponse>, BadRequest, NotFound<string>>>
        HandleGetSelfSubmission(
            string resourceId,
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
        if (course is null || !await auth.IsParticipantAsync(course.Id))
            return TypedResults.NotFound("Course not found.");

        var result = await submissionService.GetOwnDetailAsync(decoded[0], ct);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.NotFound(result.Error!.Message);
    }

    private static async Task<Ok<PaginatedResponse<AssignmentGradeResponse>>> HandleGetOwnGrades(
        [AsParameters] PageRequest page,
        AssignmentGradeService gradeService,
        CancellationToken ct)
    {
        return TypedResults.Ok((await gradeService.GetStudentSelfGradesAsync(page, ct))!);
    }

    private static async Task<Results<Ok<AssignmentGradeResponse>, BadRequest, NotFound<string>>> HandleAddGrade(
        string submissionId,
        AssignmentGradeRequest request,
        SqidsEncoder<long> sqidsEncoder,
        AssignmentGradeService gradeService)
    {
        var decoded = sqidsEncoder.Decode(submissionId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await gradeService.GradeAsync(decoded[0], request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound("Submission not found.");
    }

    private static async Task<Results<Ok<AssignmentGradeResponse>, BadRequest, NotFound<string>>> HandleUpdateGrade(
        string submissionId,
        AssignmentGradeRequest request,
        SqidsEncoder<long> sqidsEncoder,
        AssignmentGradeService gradeService)
    {
        var decoded = sqidsEncoder.Decode(submissionId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await gradeService.UpdateAsync(decoded[0], request);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound("Grade not found.");
    }

    private static async Task<Results<Ok, BadRequest, NotFound<string>>> HandleRemoveGrade(
        string submissionId,
        SqidsEncoder<long> sqidsEncoder,
        AssignmentGradeService gradeService)
    {
        var decoded = sqidsEncoder.Decode(submissionId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return await gradeService.RemoveAsync(decoded[0])
            ? TypedResults.Ok()
            : TypedResults.NotFound("Grade not found.");
    }
}
