namespace Backend.Api.Core.Types;

public record CourseResponse(
    string Id,
    string CreatorId,
    string CreatorUsername,
    string CreatorFullname,
    string Code,
    string Title,
    string? Description,
    int EnrollmentCount
) : IEntityResponse;

public record CourseProgress(
    int Completed,
    int Total
);

public record CourseStudentResponse(
    CourseResponse Info,
    CourseProgress Progress
);

public record CourseExploreResponse(
    List<CourseResponse> Featured,
    Dictionary<string, List<CourseResponse>> ByDepartment,
    List<CourseResponse> Recent
);

public record CourseRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? Code = null,
    string? Title = null,
    string? CreatorUsername = null,
    string? CreatorFullname = null,
    bool? EnrollmentOpen = null
);

public record CourseSetRequest(
    string Code,
    string Title,
    string? Description,
    bool IsPublished,
    bool EnrollmentOpen
);

public record CourseSetResponse(
    string Id,
    string CreatorId,
    string Code,
    string Title,
    string? Description,
    bool IsPublished,
    bool EnrollmentOpen
) : IEntityResponse;
