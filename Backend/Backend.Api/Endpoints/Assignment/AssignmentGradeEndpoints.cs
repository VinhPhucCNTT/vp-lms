using Backend.Api.Core.Authorization;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Content;
using Backend.Api.Services.Courses;
using Backend.Api.Services.Submissions;
using Microsoft.AspNetCore.Http.HttpResults;
using Sqids;

namespace Backend.Api.Endpoints.Assignment;

public static class AssignmentGradeEndpoints
{
    public static void AddAssignmentGradeEndpoints(this IEndpointRouteBuilder route)
    {
        var grading = route.MapGroup("/api/assignment-grade");

        grading.MapGet("{activityId}", HandleGetById).RequireAuthorization();
    }

    private static async Task<Results<Ok<AssignmentGradeResponse>, BadRequest, NotFound<string>>> HandleGetById(
        string activityId,
        SqidsEncoder<long> sqidsEncoder,
        IAssignmentGradeService assignGradeService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(activityId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await assignGradeService.GetGradeAsync(decoded[0], ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound("Grade not found.");
    }
}
