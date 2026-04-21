namespace Backend.Features.Courses.Dtos;

public record CreateCourseDto(
    string Title,
    string? Description,
    string? ThumpnailUrl,
    bool IsPublished
); // NOTE: InstructorId is acquired from token.
