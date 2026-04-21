using Backend.Models.Lessons;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class LessonRepository(AppDbContext db) : ILessonRepository
{
    private readonly AppDbContext _db = db;

    public async Task<Lesson?> GetByIdAsync(Guid id)
    {
        return await _db.Lessons.FirstOrDefaultAsync(x => x.Id == id);
    }

    public void Add(Lesson lesson)
    {
        _db.Add(lesson);
    }

    public void Update(Lesson lesson)
    {
        _db.Update(lesson);
    }

    public void Remove(Lesson lesson)
    {
        _db.Remove(lesson);
    }
}
