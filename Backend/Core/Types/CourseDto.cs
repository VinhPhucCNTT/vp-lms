namespace Backend.Core.Types;

public record CourseResponse(
    string CreatorSqid,
    string CreatorUserName,
    string Title,
    string? ThumbnailUrl,
    bool EnrollmentOpen
);

public record CourseDetailResponse(
    string CreatorSqid,
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
    string CourseSqid,
    string CreatorSqid,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsPublished,
    bool EnrollmentOpen
);
