using Backend.Models.Lessons;

namespace Backend.Data.Repositories;

public interface ILessonRepository
{
    Task<Lesson?> GetByIdAsync(Guid id);

    void Add(Lesson lesson);

    void Update(Lesson lesson);

    void Remove(Lesson lesson);
}
