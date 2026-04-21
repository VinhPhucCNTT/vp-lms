using System.Security.Claims;
using Backend.Features.Courses.Dtos;
using Backend.Features.Courses.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Backend.Endpoints;

public static class CourseEndpoints
{
    public static void AddCourseEndpoints(this IEndpointRouteBuilder app)
    {
        var courses = app.MapGroup("/api/courses");

        courses.MapGet("/", GetAllCourses)
            .RequireAuthorization(p => p.RequireRole("Instructor", "Student"));
        courses.MapGet("/{id:guid}", GetCourseById)
            .RequireAuthorization(p => p.RequireRole("Instructor", "Student"));
        courses.MapGet("/{id:guid}/content", GetCourseContent)
            .RequireAuthorization(p => p.RequireRole("Instructor", "Student"));

        courses.MapPut("/", CreateCourse)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        courses.MapPost("/{id:guid}", UpdateCourse)
            .RequireAuthorization(p => p.RequireRole("Instructor"));

        courses.MapDelete("/{id:guid}", DeleteCourse)
            .RequireAuthorization(p => p.RequireRole("Instructor"));
    }

    private static async Task<Ok<List<ViewCourseDto>>> GetAllCourses(
        ICourseService service)
    {
        return TypedResults.Ok(await service.GetAllAsync());
    }

    private static async Task<Results<Ok<ViewCourseDto>, NotFound>> GetCourseById(
        Guid id,
        ICourseService service)
    {
        var course = await service.GetByIdAsync(id);

        return course is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(course);
    }

    private static async Task<Results<Ok<CourseContentDto>, NotFound>> GetCourseContent(
        Guid id,
        ICourseService service)
    {
        var content = await service.GetFullCourseContentAsync(id);

        return TypedResults.Ok(content);
    }

    private static async Task<Ok<Guid>> CreateCourse(
        CreateCourseDto dto,
        ClaimsPrincipal user,
        ICourseService service)
    {
        var instructorId = Guid.Parse(
            user.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var id = await service.CreateAsync(dto, instructorId);

        return TypedResults.Ok(id);
    }

    private static async Task<NoContent> UpdateCourse(
        Guid id,
        UpdateCourseDto dto,
        ICourseService service)
    {
        await service.UpdateAsync(id, dto);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteCourse(
        Guid id,
        ICourseService service)
    {
        await service.DeleteAsync(id);

        return TypedResults.NoContent();
    }
}
