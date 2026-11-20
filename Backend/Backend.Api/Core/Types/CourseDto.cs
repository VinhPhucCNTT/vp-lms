namespace Backend.Api.Core.Types;

public record CourseResponse(
    string Id,
    string InstructorId,
    string Title,
    string Description,
    string? BackgroundFileId
) : IEntityResponse;

public record CourseRecentResponse(
    string CourseId,
    string CourseName,
    string? BackgroundFileId,
    DateTime LastAccessedAt
);

public record CourseDetailResponse(
    string Id,
    string InstructorId,
    string InstructorFirstName,
    string InstructorLastName,
    string? BackgroundFileId,
    // string Code,
    string Title,
    string? Description,
    int EnrollmentCount
) : IEntityResponse;

public record StudentCourseResponse(
    CourseDetailResponse Info,
    CourseProgress Progress
);

public record InstructorCourseResponse(
    CourseDetailResponse Info,
    bool IsPublished
);

public record ExploreCourseResponse(
    List<CourseDetailResponse> Featured,
    Dictionary<string, List<CourseDetailResponse>> ByDepartment,
    List<CourseDetailResponse> Recent
);

public record CourseProgress(
    int Completed,
    int Total
);

public record CourseQueryRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? Title = null,
    string? InstructorFirstName = null,
    string? InstructorLastName = null,
    bool? EnrollmentOpen = null
);

// public record CourseCreateRequest(
//     // string Code,
//     string Title,
//     string? Description,
//     bool EnrollmentOpen,
//     IFormFile? BackgroundFile = null
// );

public class CourseCreateRequest {
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public bool EnrollmentOpen { get; set; }
    public IFormFile? BackgroundFile { get; set; }
}

public record CoursePatchRequest(
    // string? Code,
    string? Title,
    string? Description,
    bool? EnrollmentOpen,
    IFormFile? BackgroundFile
);

public record CourseSetResponse(
    string Id,
    string InstructorId,
    // string Code,
    string Title,
    string? Description,
    bool IsPublished,
    bool EnrollmentOpen,
    string? BackgroundFileId
) : IEntityResponse;
