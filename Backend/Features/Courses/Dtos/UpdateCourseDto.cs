namespace Backend.Features.Courses.Dtos;

public record UpdateCourseDto(
    string Title,
    string? Description,
    string? ThumpnailUrl,
    bool IsPublished
);
