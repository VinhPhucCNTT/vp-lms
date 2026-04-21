using Backend.Features.Modules.Dtos;
using Backend.Features.Modules.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Endpoints;

public static class ModuleEndpoints
{
    public static void AddModuleEndpoints(
        this IEndpointRouteBuilder app)
    {
        app.MapPut("/api/courses/{courseId:guid}/modules", CreateModule)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapGet("/api/courses/{courseId:guid}/modules", GetModules);

        app.MapPost("/api/modules/{id:guid}", UpdateModule)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        app.MapDelete("/api/modules/{id:guid}", DeleteModule)
            .RequireAuthorization(p => p.RequireRole("Instructor"));
    }

    private static async Task<Ok<Guid>> CreateModule(
        Guid courseId,
        CreateModuleDto dto,
        IModuleService service)
    {
        return TypedResults.Ok(
            await service.CreateAsync(courseId, dto));
    }

    private static async Task<Ok<List<ViewModuleDto>>> GetModules(
        Guid courseId,
        IModuleService service)
    {
        return TypedResults.Ok(
            await service.GetByCourseAsync(courseId));
    }

    private static async Task<NoContent> UpdateModule(
        Guid id,
        UpdateModuleDto dto,
        IModuleService service)
    {
        await service.UpdateAsync(id, dto);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteModule(
        Guid id,
        IModuleService service)
    {
        await service.DeleteAsync(id);

        return TypedResults.NoContent();
    }
}
