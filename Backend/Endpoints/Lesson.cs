using Backend.Features.Lessons.Dtos;
using Backend.Features.Lessons.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Endpoints;

public static class LessonEndpoints
{
    public static void AddLessonEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/activities/{activityId:guid}/lessons", Create)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapGet("/api/lessons/{id:guid}", Get);

        app.MapPost("/api/lessons/{id:guid}", Update)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapDelete("/api/lessons/{id:guid}", Delete)
            .RequireAuthorization(p => p.RequireRole("Instructor"));
    }

    private static async Task<Ok<Guid>> Create(
        Guid activityId,
        CreateLessonDto dto,
        ILessonService service)
    {
        return TypedResults.Ok(
            await service.CreateAsync(activityId, dto));
    }

    private static async Task<Results<Ok<ViewLessonDto>, NotFound>> Get(
        Guid id,
        ILessonService service)
    {
        var lesson = await service.GetByIdAsync(id);
        if (lesson is null)
            return TypedResults.NotFound();

        return TypedResults.Ok(lesson);
    }

    private static async Task<NoContent> Update(
        Guid id,
        UpdateLessonDto dto,
        ILessonService service)
    {
        await service.UpdateAsync(id, dto);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> Delete(
        Guid id,
        ILessonService service)
    {
        await service.DeleteAsync(id);

        return TypedResults.NoContent();
    }
}
