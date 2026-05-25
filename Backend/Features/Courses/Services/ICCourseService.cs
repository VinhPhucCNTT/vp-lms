using Backend.Common;
using Backend.Features.Courses.Types;

namespace Backend.Features.Courses.Services;

public interface ICCourseService
{
    Task<MResult<CourseSetResponse, CourseSetError>> CreateCourseAsync(Guid userId, CourseSetRequest dto);

    Task<MResult<CourseSetResponse, CourseSetError>> UpdateCourseAsync(Guid userId, Guid courseId, CourseSetRequest dto);

    Task<CourseDeleteStatus> DeleteCourseAsync(Guid userId, Guid courseId);
}
