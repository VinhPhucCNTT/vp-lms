namespace Backend.Core.Types;

public record CourseResponse(
    Guid CreatorId,
    string CreatorUserName,
    string Title,
    string? ThumbnailUrl,
    bool AllowAnonymousAccess,
    bool EnrollmentOpen
);

public record CourseDetailResponse(
    Guid CreatorId,
    UserResponse Creator,
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
