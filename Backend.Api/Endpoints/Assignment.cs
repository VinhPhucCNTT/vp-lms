using Microsoft.AspNetCore.Http.HttpResults;

using Backend.Api.Core.Types;
using Sqids;

namespace Backend.Api.Endpoints;

public static class AssignmentEndpoints
{
    public static void AddAssignmentEndpoints(this IEndpointRouteBuilder route)
    {
        var assignment = route.MapGroup("/api/assignment").RequireAuthorization();

        // assignment.MapGet("{resourceId}", HandleGetById);
        // assignment.MapPost("{moduleId}", HandleCreate);
        // assignment.MapPut("{resourceId}", HandleUpdate);
        //
        // assignment.MapPost("{resourceId}/submit", HandleSubmit);
        // assignment.MapGet("{resourceId}/file", HandleGetFile);
        //
        // assignment.MapPost("{submissionId}/grade", HandleGrade);
    }
}
