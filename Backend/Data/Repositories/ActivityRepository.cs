using Backend.Models.Courses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class ActivityRepository(AppDbContext db) : IActivityRepository
{
    private readonly AppDbContext _db = db;

    public async Task<CourseActivity?> GetByIdAsync(Guid id)
    {
        return await _db.CourseActivities.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<CourseActivity>> GetByModuleIdAsync(Guid moduleId)
    {
        return await _db.CourseActivities
            .AsSplitQuery()
            .Where(x => x.ModuleId == moduleId)
            .Include(x => x.Lesson)
            .Include(x => x.Assessment)
            .Include(x => x.Assignment)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync();
    }

    public async Task<int> CountActivitiesAsync(CourseModule module)
    {
        return await _db.CourseActivities
            .Where(x => x.ModuleId == module.Id)
            .CountAsync();
    }

    public async void Add(CourseActivity activity)
    {
        _db.CourseActivities.Add(activity);
    }

    public void Update(CourseActivity activity)
    {
        _db.CourseActivities.Update(activity);
    }

    public void Remove(CourseActivity activity)
    {
        _db.CourseActivities.Remove(activity);
    }
}
