using Backend.Common;
using Backend.Features.Courses.Types;

namespace Backend.Features.Courses.Services;

public interface ICourseService
{
    Task<CourseResponse?> GetCourseByIdAsync(Guid courseId);

    Task<QueryResponse<CourseResponse>> QueryCoursesAsync(CourseRequest query);

    Task<CourseSetResult> CreateCourseAsync(Guid userId, CourseSetRequest dto);

    Task<CourseSetResult> UpdateCourseAsync(Guid userId, Guid courseId, CourseSetRequest dto);

    Task<CourseDeleteStatus> DeleteCourseAsync(Guid userId, Guid courseId);
}
