namespace Backend.Services.Courses.Types;

public record CourseResponse(
    Guid CreatorId,
    string CreatorUserName,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool AllowAnonymousAccess,
    bool EnrollmentOpen
);

public record CourseRequest(
    int PageNumber,
    int PageSize,
    string? Title,
    string? CreatorUserName,
    bool? AllowAnonymousAccess,
    bool? EnrollmentOpen
);
