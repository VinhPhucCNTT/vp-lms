namespace Backend.Services.Courses.Types;

public record CCourseResponse(
    Guid CreatorId,
    string CreatorUserName,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool AllowAnonymousAccess,
    bool EnrollmentOpen,
    bool IsPublished
);

public record CCourseRequest(
    int PageNumber,
    int PageSize,
    string? Title,
    string? CreatorUserName,
    bool? AllowAnonymousAccess,
    bool? EnrollmentOpen,
    bool? IsPublished
);

public record CCourseSetRequest(
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsPublished,
    bool AllowAnonymousAccess,
    bool EnrollmentOpen
);

public record CCourseSetResponse(
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsPublished,
    bool AllowAnonymousAccess,
    bool EnrollmentOpen
);
