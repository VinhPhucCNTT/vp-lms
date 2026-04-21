using System.Security.Claims;
using Backend.Features.Assignments.Dtos;
using Backend.Features.Assignments.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Endpoints;

public static class AssignmentEndpoints
{
    public static void AddAssignmentEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/activities/{activityId:guid}/assignments", Create)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapGet("/api/assignments/{id:guid}", Get)
            .RequireAuthorization(p => p.RequireRole("Instructor", "Student"));

        app.MapPost("/api/assignments/{id:guid}", Update)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapPost("/api/assignments/{id:guid}/submit", Submit)
            .RequireAuthorization(p => p.RequireRole("Student"));
    }

    private static async Task<Ok<Guid>> Create(
        Guid activityId,
        CreateAssignmentDto dto,
        IAssignmentService service)
    {
        return TypedResults.Ok(
            await service.CreateAsync(activityId, dto));
    }

    private static async Task<Results<Ok<ViewAssignmentDto>, NotFound>> Get(
        Guid id,
        IAssignmentService service)
    {
        var assignment = await service.GetByIdAsync(id);
        if (assignment is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(assignment);
    }

    private static async Task<Results<NoContent, NotFound>> Update(
        Guid id,
        UpdateAssignmentDto dto,
        IAssignmentService service)
    {
        var updated = await service.UpdateAsync(id, dto);
        return updated
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<NoContent> Submit(
        Guid id,
        SubmitAssignmentDto dto,
        ClaimsPrincipal user,
        IAssignmentService service)
    {
        var studentId = Guid.Parse(
            user.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        await service.SubmitAsync(id, studentId, dto);

        return TypedResults.NoContent();
    }
}
