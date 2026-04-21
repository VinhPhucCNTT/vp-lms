using System.Security.Claims;
using Backend.Features.Assessments.Dtos.Modify;
using Backend.Features.Assessments.Dtos.Submission;
using Backend.Features.Assessments.Dtos.View;
using Backend.Features.Assessments.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Endpoints;

public static class AssessmentEndpoints
{
    public static void AddAssessmentEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/activities/{activityId:guid}/assessments", Create)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapGet("/api/assessments/{id:guid}", Get)
            .RequireAuthorization(p => p.RequireRole("Instructor", "Student"));

        app.MapPost("/api/assessments/{id:guid}", Update)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapGet("/api/assessments/{id:guid}/questions", GetQuestions)
            .RequireAuthorization(p => p.RequireRole("Instructor", "Student"));

        app.MapPost("/api/assessments/{id:guid}/questions", AddQuestion)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapPost("/api/questions/{id:guid}", UpdateQuestion)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapDelete("/api/questions/{id:guid}", DeleteQuestion)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapPost("/api/assessments/{id:guid}/submit", Submit)
            .RequireAuthorization(p => p.RequireRole("Student"));
    }

    private static async Task<Ok<Guid>> Create(
        Guid activityId,
        CreateAssessmentDto dto,
        IAssessmentService service)
    {
        return TypedResults.Ok(
            await service.CreateAsync(activityId, dto));
    }

    private static async Task<Results<Ok<ViewAssessmentDto>, NotFound>> Get(
        Guid id,
        IAssessmentService service)
    {
        var assessment = await service.GetByIdAsync(id);
        if (assessment is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(assessment);
    }

    private static async Task<Results<Ok<List<ViewQuestionDto>>, NotFound>> GetQuestions(
        Guid id,
        IAssessmentService service)
    {
        var list = await service.GetAssessmentQuestionsAsync(id);
        return list is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(list);
    }

    private static async Task<Results<NoContent, NotFound>> Update(
        Guid id,
        UpdateAssessmentDto dto,
        IAssessmentService service)
    {
        var updated = await service.UpdateAsync(id, dto);
        return updated
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<Results<Ok<Guid>, NotFound>> AddQuestion(
        Guid id,
        CreateAssessmentQuestionBody dto,
        IAssessmentService service)
    {
        var questionId = await service.AddQuestionAsync(id, dto);
        return questionId is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(questionId.Value);
    }

    private static async Task<Results<NoContent, NotFound>> UpdateQuestion(
        Guid id,
        UpdateQuestionDto dto,
        IAssessmentService service)
    {
        var updated = await service.UpdateQuestionAsync(id, dto);
        return updated
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteQuestion(
        Guid id,
        IAssessmentService service)
    {
        var deleted = await service.DeleteQuestionAsync(id);
        return deleted
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<Results<Ok<AssessmentResultDto>, NotFound>> Submit(
        Guid id,
        SubmitAssessmentDto dto,
        ClaimsPrincipal user,
        IAssessmentService service)
    {
        var studentId = Guid.Parse(
            user.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var result = await service.SubmitAttemptAsync(
            id,
            studentId,
            dto);

        return result is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(result);
    }
}
