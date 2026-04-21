using Backend.Models.Courses;

namespace Backend.Data.Repositories;

public interface IEnrollmentRepository
{
    Task AddAsync(CourseEnrollment enrollment);

    Task<CourseEnrollment?> GetByStudentAndCourseAsync(
        Guid studentId,
        Guid courseId);

    Task<List<CourseEnrollment>> GetByStudentIdAsync(Guid studentId);

    void Remove(CourseEnrollment enrollment);
}
