using Backend.Models.Courses;

namespace Backend.Data.Repositories;

public interface IActivityRepository
{
    Task<CourseActivity?> GetByIdAsync(Guid id);

    Task<List<CourseActivity>> GetByModuleIdAsync(Guid moduleId);

    Task<int> CountActivitiesAsync(CourseModule module);

    void Add(CourseActivity activity);

    void Update(CourseActivity activity);

    void Remove(CourseActivity activity);
}
