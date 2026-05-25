using Backend.Common;
using Backend.Features.Courses.Types;

namespace Backend.Features.Courses.Services;

public interface ICourseService
{
    Task<CourseResponse?> GetCourseByIdAsync(Guid courseId);

    Task<QueryResponse<CourseResponse>> QueryCoursesAsync(CourseRequest query);
}
