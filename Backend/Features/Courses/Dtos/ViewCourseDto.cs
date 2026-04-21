namespace Backend.Features.Courses.Dtos;

public record ViewCourseDto(
    Guid Id,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsPublished,
    Guid InstructorId,
    string? InstructorUserName,
    string InstructorFullName,
    int ModuleCount,
    int EnrollmentCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
