using Backend.Models.Courses;
using Backend.Models.Users;

namespace Backend.Data.Repositories;

public interface ICourseRepository
{
    Task<Course?> GetByIdAsync(Guid id);

    Task<Course?> GetDeletedByIdAsync(Guid id);

    Task<Course?> GetFullContentByIdAsync(Guid courseId);

    Task<int> CountEnrollmentsAsync(Course course);

    Task<List<Course>> GetAllAsync();

    Task<List<Course>> GetByInstructorAsync(ApplicationUser instructor);

    Task<List<CourseModule>> GetModulesAsync(Course course);

    Task AddAsync(Course course);

    void Update(Course course);

    void Remove(Course course);

    void Restore(Course course);

    void HardDelete(Course course);

    Task<bool> ExistsAsync(Guid id);
}
