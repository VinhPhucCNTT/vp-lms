using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
using Sqids;
using Backend.Api.Services.Content;
using Backend.Api.Services.Courses;
using Backend.Api.Core.Authorization;
using Backend.Api.Services.Submissions;
using Backend.Api.Core.Common;

namespace Backend.Api.Endpoints.Assignment;

public static class AssignmentGradeEndpoints
{
    public static void AddAssignmentGradeEndpoints(this IEndpointRouteBuilder route)
    {
        var grading = route.MapGroup("/api/assignment-grade");

        grading.MapGet("{resourceId}", HandleGetById).RequireAuthorization();
    }

    private static async Task<Results<Ok<AssignmentGradeResponse>, BadRequest, NotFound<string>>> HandleGetById(
        string resourceId,
        SqidsEncoder<long> sqidsEncoder,
        AssignmentGradeService gradeService,
        CancellationToken ct)
    {
        var decoded = sqidsEncoder.Decode(resourceId);
        if (decoded.Count != 1)
            return TypedResults.BadRequest();

        var result = await gradeService.GetGradeAsync(decoded[0], ct);
        return result is not null
            ? TypedResults.Ok(result)
            : TypedResults.NotFound("Grade not found.");
    }
}
