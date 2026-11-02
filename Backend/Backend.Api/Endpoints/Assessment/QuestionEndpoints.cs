using Backend.Api.Core.Types;
using Backend.Api.Services.Assessments;
using Microsoft.AspNetCore.Http.HttpResults;
using Sqids;

namespace Backend.Api.Endpoints.Assessment;

public static class QuestionEndpoints
{
    public static void AddQuestionEndpoints(this IEndpointRouteBuilder route)
    {
        var question = route.MapGroup("api/assessment")
            .WithTags("AssessmentQuestions")
            .RequireAuthorization();

        question.MapGet("{resourceId}/question", HandleGetQuestions);
    }

    private static async Task<Results<Ok<List<QuestionResponse>>, BadRequest>> HandleGetQuestions(
        string resourceId,
        SqidsEncoder<long> sqidsEncoder,
        IQuestionService questionService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        return TypedResults.Ok(await questionService.GetForAssessmentAsync(decoded[0], ct));
    }
}
