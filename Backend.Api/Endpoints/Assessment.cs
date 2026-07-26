using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
using Sqids;

namespace Backend.Api.Endpoints;

public static class AssessmentEndpoints
{
    public static void AddAssessmentEndpoints(this IEndpointRouteBuilder route)
    {
        var assessment = route.MapGroup("/api/assessment").RequireAuthorization();

        assessment.MapGet("{resourceId}", HandleGetById);
        assessment.MapPut("{moduleId}", HandleCreate);
        assessment.MapPut("{resourceId}", HandleUpdate);

        assessment.MapPost("{resourceId}/start", HandleStart);
        assessment.MapPost("{resourceId}/set-answer", HandleSetAnswer);
        assessment.MapPost("{resourceId}/submit", HandleSubmit);

        assessment.MapPost("{resourceId}/grade/{answerId}", HandleGrade);
        assessment.MapPut("{resourceId}/build/add-question", HandleAddQuestion);
        assessment.MapPut("{resourceId}/build/add-question", HandleAddQuestion);

        assessment.MapGet("{resourceId}/time", HandleGetTime);
        assessment.MapGet("{resourceId}/restore", HandleRestore);
        assessment.MapGet("{resourceId}/attempt/{attemptId}", HandleGetAttempt);
    }
}
