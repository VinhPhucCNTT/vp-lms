using Backend.Features.Courses.Dtos;

namespace Backend.Features.Courses.Services;

public interface ICourseService
{
    Task<Guid> CreateAsync(CreateCourseDto dto, Guid instructorId);

    Task<ViewCourseDto?> GetByIdAsync(Guid id);

    Task<List<ViewCourseDto>> GetAllAsync();

    Task<List<ViewCourseDto>> GetInstructorCoursesAsync(Guid instructorId);

    Task<CourseContentDto?> GetFullCourseContentAsync(Guid courseId);

    Task<bool> UpdateAsync(Guid id, UpdateCourseDto dto);

    Task<bool> DeleteAsync(Guid id);

    Task<bool> HardDeleteAsync(Guid id);

    Task<bool> RestoreAsync(Guid id);
}
