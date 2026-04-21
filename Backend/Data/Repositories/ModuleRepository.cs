using Backend.Models.Courses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class ModuleRepository(AppDbContext db) : IModuleRepository
{
    private readonly AppDbContext _db = db;

    public async Task<CourseModule?> GetByIdAsync(Guid id)
    {
        return await _db.CourseModules.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<CourseModule>> GetByCourseIdAsync(Guid courseId)
    {
        return await _db.CourseModules
            .Where(x => x.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<int> CountModulesAsync(Course course)
    {
        return await _db.CourseModules
            .Where(x => x.CourseId == course.Id)
            .CountAsync();
    }

    public async void Add(CourseModule module)
    {
        _db.CourseModules.Add(module);
    }

    public async void Update(CourseModule module)
    {
        _db.CourseModules.Update(module);
    }

    public async void Remove(CourseModule module)
    {
        _db.CourseModules.Remove(module);
    }
}
