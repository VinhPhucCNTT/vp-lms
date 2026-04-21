using Backend.Models.Courses;
using Backend.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class CourseRepository(AppDbContext db) : ICourseRepository
{
    private readonly AppDbContext _db = db;

    public async Task<Course?> GetByIdAsync(Guid id)
    {
        return await _db.Courses
            .Include(x => x.Modules)
            .Include(x => x.Instructor)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Course?> GetDeletedByIdAsync(Guid id)
    {
        return await _db.Courses
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsDeleted);
    }

    public async Task<Course?> GetFullContentByIdAsync(Guid courseId)
    {
        return await _db.Courses
            .AsSplitQuery()
            .Include(c => c.Modules.OrderBy(m => m.OrderIndex))
                .ThenInclude(m => m.Activities.OrderBy(a => a.OrderIndex))
                    .ThenInclude(a => a.Lesson)
            .Include(c => c.Modules)
                .ThenInclude(m => m.Activities)
                    .ThenInclude(a => a.Assessment)
            .Include(c => c.Modules)
                .ThenInclude(m => m.Activities)
                    .ThenInclude(a => a.Assignment)
            .FirstOrDefaultAsync(c => c.Id == courseId);
    }

    public async Task<int> CountEnrollmentsAsync(Course course)
    {
        return await _db.CourseEnrollments
            .Where(e => e.CourseId == course.Id)
            .CountAsync();
    }

    public async Task<List<Course>> GetAllAsync()
    {
        return await _db.Courses
            .Include(c => c.Instructor)
            .ToListAsync();
    }

    public async Task<List<Course>> GetByInstructorAsync(ApplicationUser intructor)
    {
        return await _db.Courses
            .Include(c => c.Instructor)
            .Where(x => x.InstructorId == intructor.Id)
            .ToListAsync();
    }

    public async Task<List<CourseModule>> GetModulesAsync(Course course)
    {
        return await _db.CourseModules
            .Where(x => x.CourseId == course.Id)
            .ToListAsync();
    }

    public async Task AddAsync(Course course)
    {
        await _db.Courses.AddAsync(course);
    }

    public void Update(Course course)
    {
        _db.Courses.Update(course);
    }

    public void Remove(Course course)
    {
        _db.Courses.Remove(course);
    }

    public void Restore(Course course)
    {
        course.IsDeleted = false;
        course.DeletedAt = null;

        _db.Courses.Update(course);
    }

    public void HardDelete(Course course)
    {
        _db.Courses.IgnoreQueryFilters();

        _db.Entry(course).State = EntityState.Deleted;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _db.Courses.AnyAsync(x => x.Id == id);
    }
}
