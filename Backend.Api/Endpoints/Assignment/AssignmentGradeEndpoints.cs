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
}
