namespace Backend.Core.Types;

public record CourseResponse(
    string Id,
    string CreatorId,
    string CreatorUserName,
    string Title,
    string? ThumbnailUrl,
    bool EnrollmentOpen
) : IEntityResponse;

public record CourseDetailResponse(
    string CreatorId,
    UserResponse Creator,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool EnrollmentOpen
    // TAPermissionResponse Permissions
);

public record CourseRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? Title = null,
    string? CreatorUserName = null,
    bool? EnrollmentOpen = null
);

public record CourseSetRequest(
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsPublished,
    bool EnrollmentOpen
);

public record CourseSetResponse(
    string Id,
    string CreatorId,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsPublished,
    bool EnrollmentOpen
) : IEntityResponse;
