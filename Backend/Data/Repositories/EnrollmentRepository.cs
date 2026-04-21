using Backend.Models.Courses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Data.Repositories;

public class EnrollmentRepository(AppDbContext db) : IEnrollmentRepository
{
    private readonly AppDbContext _db = db;

    public async Task AddAsync(CourseEnrollment enrollment)
    {
        await _db.CourseEnrollments.AddAsync(enrollment);
    }

    public async Task<CourseEnrollment?> GetByStudentAndCourseAsync(
        Guid studentId,
        Guid courseId)
    {
        return await _db.CourseEnrollments
            .FirstOrDefaultAsync(x =>
                x.UserId == studentId &&
                x.CourseId == courseId);
    }

    public async Task<List<CourseEnrollment>> GetByStudentIdAsync(Guid studentId)
    {
        return await _db.CourseEnrollments
            .Include(x => x.Course)
            .ThenInclude(x => x.Instructor)
            .Where(x => x.UserId == studentId)
            .ToListAsync();
    }

    public void Remove(CourseEnrollment enrollment)
    {
        _db.CourseEnrollments.Remove(enrollment);
    }
}
