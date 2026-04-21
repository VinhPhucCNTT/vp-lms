using Backend.Features.Activities.Dtos;
using Backend.Features.Activities.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Endpoints;

public static class ActivityEndpoints
{
    public static void AddActivityEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/modules/{moduleId:guid}/activities", Create)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapGet("/api/modules/{moduleId:guid}/activities", Get)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapPost("/api/activities/{id:guid}", Update)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapDelete("/api/activities/{id:guid}", Delete)
            .RequireAuthorization(p => p.RequireRole("Instructor"));
    }

    private static async Task<Ok<Guid>> Create(
        Guid moduleId,
        CreateActivityDto dto,
        IActivityService service)
    {
        return TypedResults.Ok(
            await service.CreateAsync(moduleId, dto));
    }

    private static async Task<Ok<List<ViewActivityDto>>> Get(
        Guid moduleId,
        IActivityService service)
    {
        return TypedResults.Ok(
            await service.GetByModuleAsync(moduleId));
    }

    private static async Task<Results<NoContent, NotFound>> Update(
        Guid id,
        UpdateActivityDto dto,
        IActivityService service)
    {
        var updated = await service.UpdateAsync(id, dto);
        return updated
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<NoContent> Delete(
        Guid id,
        IActivityService service)
    {
        await service.DeleteAsync(id);

        return TypedResults.NoContent();
    }
}
