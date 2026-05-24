namespace Backend.Features.Courses.Types;

public record CourseSetRequest(
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsPublished,
    bool AllowAnonymousAccess,
    bool EnrollmentOpen
);

public record CourseSetResponse(
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsPublished,
    bool AllowAnonymousAccess,
    bool EnrollmentOpen
);

public record CourseQueryRequest(
    int PageNumber,
    int PageSize,
    string? Title,
    string? CreatorUserName,
    bool? AllowAnonymousAccess,
    bool? EnrollmentOpen
);

public record CourseResponse(
    Guid CreatorId,
    string CreatorUserName,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool AllowAnonymousAccess,
    bool EnrollmentOpen
);
