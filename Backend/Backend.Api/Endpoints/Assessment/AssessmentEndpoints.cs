using System.Text.Json;
using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
using Sqids;
using Backend.Api.Services.Courses;
using Backend.Api.Core.Authorization;
using Backend.Api.Services.Content;
using Backend.Api.Services.Assessments;
using Backend.Persistence.Entities.Assessments;

namespace Backend.Api.Endpoints.Assessment;

public static class AssessmentEndpoints
{
    public static void AddAssessmentEndpoints(this IEndpointRouteBuilder route)
    {
        var assessment = route.MapGroup("/api/assessments").RequireAuthorization();

        assessment.MapGet("/", HandleQuery).RequireAuthorization("IsStudent");
        assessment.MapGet("{resourceId}", HandleGetById).RequireAuthorization("IsStudent");
        assessment.MapPost("{resourceId}/start", HandleStart).RequireAuthorization("IsStudent");
        assessment.MapGet("{resourceId}/attempt/{attemptId}", HandleGetAttempt).RequireAuthorization("IsStudent");
        assessment.MapPost("{resourceId}/attempt/{attemptId}/question/{attemptQuestionId}/answer", HandleSaveAnswer)
            .RequireAuthorization("IsStudent");
        assessment.MapPost("{resourceId}/attempt/{attemptId}/submit", HandleSubmit)
            .RequireAuthorization("IsStudent");

        assessment.MapPost("{moduleId}", HandleCreate);
        assessment.MapPut("{resourceId}", HandleUpdate);

        assessment.MapPost("{resourceId}/set-publish", HandleSetPublish);

        // assessment.MapPost("{resourceId}/set-answer", HandleSetAnswer);

        // assessment.MapPost("{resourceId}/grade/{answerId}", HandleGrade);
        // assessment.MapPut("{resourceId}/build/add-question", HandleAddQuestion);
        // assessment.MapPut("{resourceId}/build/add-question", HandleAddQuestion);

        // assessment.MapGet("{resourceId}/time", HandleGetTime);
        // assessment.MapGet("{resourceId}/restore", HandleRestore);
        // assessment.MapGet("{resourceId}/attempt/{attemptId}", HandleGetAttempt);
    }

    private static async Task<Ok<List<AssessmentListResponse>>> HandleQuery(
        AssessmentService assessmentService,
        CancellationToken ct)
    {
        return TypedResults.Ok(await assessmentService.QueryAsync(ct));
    }

    private static async Task<IResult> HandleStart(
        string resourceId,
        SqidsEncoder<long> sqidsEncoder,
        AssessmentService assessmentService,
        AssessmentAttemptService attemptService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var assessment = await assessmentService.GetAsync(decoded[0], ct);
        if (assessment is null)
            return TypedResults.NotFound("Assessment not found.");

        var result = await attemptService.StartAsync(assessment.Id, ct);
        if (result.IsSuccess)
        {
            var attempt = result.Value!;
            return TypedResults.Ok(new AssessmentAttemptResponse(
                sqidsEncoder.Encode(attempt.Id),
                attempt.StartedAt,
                attempt.SubmittedAt,
                attempt.TotalScore,
                null,
                attempt.AttemptNumber));
        }

        var error = result.Error!;
        return error.Code == "assessment.not_enrolled"
            ? TypedResults.Forbid()
            : error.Code == "assessment.not_found"
                ? TypedResults.NotFound("Assessment not found.")
                : TypedResults.BadRequest(error);
    }

    private static async Task<IResult> HandleGetAttempt(
        string resourceId,
        string attemptId,
        SqidsEncoder<long> sqidsEncoder,
        AssessmentService assessmentService,
        AssessmentAttemptService attemptService,
        CancellationToken ct)
    {
        var resourceDecoded = sqidsEncoder.Decode(resourceId);
        var attemptDecoded = sqidsEncoder.Decode(attemptId);
        if (resourceDecoded.Count != 1 || attemptDecoded.Count != 1)
            return TypedResults.BadRequest();

        var assessment = await assessmentService.GetAsync(resourceDecoded[0], ct);
        if (assessment is null)
            return TypedResults.NotFound("Assessment not found.");

        var result = await attemptService.GetAsync(attemptDecoded[0], ct);
        if (!result.IsSuccess || result.Value!.AssessmentId != assessment.Id)
            return TypedResults.NotFound("Assessment attempt not found.");

        return TypedResults.Ok(ToDetailResponse(result.Value, sqidsEncoder));
    }

    private static async Task<IResult> HandleSaveAnswer(
        string resourceId,
        string attemptId,
        string attemptQuestionId,
        SaveAttemptAnswerRequest request,
        SqidsEncoder<long> sqidsEncoder,
        AssessmentService assessmentService,
        AssessmentAttemptService attemptService,
        CancellationToken ct)
    {
        var resourceDecoded = sqidsEncoder.Decode(resourceId);
        var attemptDecoded = sqidsEncoder.Decode(attemptId);
        var attemptQuestionDecoded = sqidsEncoder.Decode(attemptQuestionId);
        if (resourceDecoded.Count != 1 ||
            attemptDecoded.Count != 1 ||
            attemptQuestionDecoded.Count != 1)
        {
            return TypedResults.BadRequest();
        }

        var assessment = await assessmentService.GetAsync(resourceDecoded[0], ct);
        if (assessment is null)
            return TypedResults.NotFound("Assessment not found.");

        var result = await attemptService.SaveAnswerAsync(
            assessment.Id,
            attemptDecoded[0],
            attemptQuestionDecoded[0],
            request.AnswerData,
            ct);

        return result.IsSuccess
            ? TypedResults.NoContent()
            : result.Error!.Code == "attempt_question.not_found"
                ? TypedResults.NotFound("Assessment question not found.")
                : TypedResults.BadRequest(result.Error);
    }

    private static async Task<IResult> HandleSubmit(
        string resourceId,
        string attemptId,
        SqidsEncoder<long> sqidsEncoder,
        AssessmentService assessmentService,
        AssessmentAttemptService attemptService,
        CancellationToken ct)
    {
        var resourceDecoded = sqidsEncoder.Decode(resourceId);
        var attemptDecoded = sqidsEncoder.Decode(attemptId);
        if (resourceDecoded.Count != 1 || attemptDecoded.Count != 1)
            return TypedResults.BadRequest();

        var assessment = await assessmentService.GetAsync(resourceDecoded[0], ct);
        if (assessment is null)
            return TypedResults.NotFound("Assessment not found.");

        var result = await attemptService.SubmitAsync(
            assessment.Id,
            attemptDecoded[0],
            ct);
        if (!result.IsSuccess)
        {
            return result.Error!.Code == "attempt.not_found"
                ? TypedResults.NotFound("Assessment attempt not found.")
                : TypedResults.BadRequest(result.Error);
        }

        var submitted = await attemptService.GetAsync(attemptDecoded[0], ct);
        if (!submitted.IsSuccess || submitted.Value!.AssessmentId != assessment.Id)
            return TypedResults.NotFound("Assessment attempt not found.");

        return TypedResults.Ok(ToDetailResponse(submitted.Value, sqidsEncoder));
    }

    private static AssessmentAttemptDetailResponse ToDetailResponse(
        Backend.Persistence.Entities.Assessments.AssessmentAttempt attempt,
        SqidsEncoder<long> sqidsEncoder)
    {
        var questions = attempt.Questions
            .OrderBy(x => x.OrderIndex)
            .Select(x => new AssessmentAttemptQuestionResponse(
                sqidsEncoder.Encode(x.Id),
                x.AssessmentQuestion.QuestionType.ToString(),
                x.AssessmentQuestion.Text,
                GetPublicQuestionData(x.AssessmentQuestion),
                x.OrderIndex,
                x.Points,
                x.IsFlagged,
                x.Answer?.AnswerData,
                x.Answer?.AnsweredAt))
            .ToList();

        return new AssessmentAttemptDetailResponse(
            sqidsEncoder.Encode(attempt.Id),
            attempt.StartedAt,
            attempt.SubmittedAt,
            attempt.TotalScore,
            attempt.Questions.Sum(x => x.Points),
            null,
            attempt.AttemptNumber,
            attempt.Status.ToString(),
            questions);
    }

    private static JsonDocument GetPublicQuestionData(
        Backend.Persistence.Entities.Assessments.AssessmentQuestion question)
    {
        if (question.QuestionType is QuestionType.MultipleChoice or QuestionType.MultipleSelect)
        {
            var options = question.QuestionData
                .Deserialize<MultipleChoiceQuestion>()?.Options
                .Select(x => new PublicQuestionOption(x.Id, x.Text))
                .ToArray() ?? [];

            return JsonDocument.Parse(JsonSerializer.Serialize(new { options }));
        }

        // Correct answers and accepted-answer lists must never be sent to a student.
        return JsonDocument.Parse("{}");
    }

    private sealed record PublicQuestionOption(string Id, string Text);

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
