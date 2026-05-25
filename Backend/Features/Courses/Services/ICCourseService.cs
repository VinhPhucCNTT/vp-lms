using Backend.Common;
using Backend.Features.Courses.Types;

namespace Backend.Features.Courses.Services;

public interface ICCourseService
{
    Task<MResult<CCourseResponse, CCourseGetError>> GetCourseByIdAsync(Guid userId, Guid courseId);

    Task<QueryResponse<CCourseResponse>> QueryCoursesAsync(Guid userId, CCourseRequest query);

    Task<MResult<CCourseSetResponse, CCourseSetError>> CreateCourseAsync(Guid userId, CCourseSetRequest dto);

    Task<MResult<CCourseSetResponse, CCourseSetError>> UpdateCourseAsync(Guid userId, Guid courseId, CCourseSetRequest dto);

    Task<CCourseDeleteStatus> DeleteCourseAsync(Guid userId, Guid courseId);
}
