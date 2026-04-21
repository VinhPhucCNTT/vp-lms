using Backend.Features.Courses.Dtos;

namespace Backend.Features.Enrollments.Services;

public interface IEnrollmentService
{
    Task<bool> EnrollStudentAsync(Guid courseId, Guid studentId);

    Task<List<ViewCourseDto>> GetStudentCoursesAsync(Guid studentId);

    Task<bool> UnenrollStudentAsync(Guid courseId, Guid studentId);
}
