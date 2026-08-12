using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
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
}
