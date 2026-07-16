namespace Backend.Core.Types;

public record CourseResponse(
    string Id,
    string CreatorUserName,
    string CreatorFullName,
    string Code,
    string Title,
    string Description,
    string? ThumbnailUrl,
    int StudentCount
);

public record CourseStudentResponse(
    string Id,
    string CreatorUserName,
    string CreatorFullName,
    string Code,
    string Title,
    string Description,
    string? ThumbnailUrl,
    int CompletedActivities,
    int TotalActivities,
    int StudentCount,
    bool EnrollmentOpen
);

public record CourseExploreResponse(
    List<CourseResponse> FeaturedCourses,
    Dictionary<string, List<CourseResponse>> CoursesByDepartment,
    List<CourseResponse> RecentlyUpdated
);

public record CourseRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? Code = null,
    string? Title = null,
    string? CreatorUserName = null,
    string? CreatorFullName = null,
    bool? EnrollmentOpen = null
);

public record CourseSetRequest(
    string Code,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsPublished,
    bool EnrollmentOpen
);

public record CourseSetResponse(
    string Id,
    string CreatorId,
    string Code,
    string Title,
    string? Description,
    string? ThumbnailUrl,
    bool IsPublished,
    bool EnrollmentOpen
) : IEntityResponse;
