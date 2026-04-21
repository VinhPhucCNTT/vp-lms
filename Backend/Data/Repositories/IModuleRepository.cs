using Backend.Models.Courses;

namespace Backend.Data.Repositories;

public interface IModuleRepository
{
    Task<CourseModule?> GetByIdAsync(Guid id);

    Task<List<CourseModule>> GetByCourseIdAsync(Guid courseId);

    Task<int> CountModulesAsync(Course course);

    void Add(CourseModule module);

    void Update(CourseModule module);

    void Remove(CourseModule module);
}
