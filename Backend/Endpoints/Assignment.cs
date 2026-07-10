using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Core.Types;
using Sqids;

namespace Backend.Endpoints;

public static class AssignmentEndpoints
{
    public static void AddAssignmentEndpoints(this IEndpointRouteBuilder route)
    {
        var assignment = route.MapGroup("/api/assignment").RequireAuthorization();

        assignment.MapGet("{resourceId}", HandleGetById);
        assignment.MapPut("{moduleId}", HandleCreate);
        assignment.MapPost("{resourceId}", HandleUpdate);

        assignment.MapPut("{resourceId}/submit", HandleSubmit);
        assignment.MapGet("{resourceId}/file", HandleGetFile);

        assignment.MapPost("{submissionId}/grade", HandleGrade);
    }
}
