using AutoMapper;
using Backend.Core.Entities.Courses;
using Backend.Core.Types;
using Backend.Data;
using Backend.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services.Users;

public class EnrollmentService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper
    )
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;

    public async Task<EnrollmentDetailResponse?> GetEnrollmentByIdAsync(long enrollmentId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Enrollments
            .AsNoTracking()
            .Where(e => e.Id == enrollmentId)
            .Select(e => _mapper.Map<EnrollmentDetailResponse>(e))
            .FirstOrDefaultAsync();
    }

    public async Task<List<EnrollmentResponse>> GetCourseEnrollmentsAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await GetEnrollmentsAsync(db, e => e.CourseId == courseId);
    }

    public async Task<List<EnrollmentResponse>> GetUserEnrollmentsAsync(long userId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await GetEnrollmentsAsync(db, e => e.UserId == userId);
    }

    public async Task<List<EnrollmentResponse>> GetCurrentEnrollmentsAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await GetEnrollmentsAsync(db, e => e.UserId == currentUserId);
    }

    public async Task<EnrollmentResponse?> EnrollCourseAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;

        var course = await db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == courseId);
        if (course is null || course.CreatorId == currentUserId)
            return null;
        var existing = await db.Enrollments.AsNoTracking().FirstOrDefaultAsync(e => e.UserId == currentUserId && e.CourseId == courseId);
        if (existing is not null)
            return null;

        var enrollment = new Enrollment
        {
            CourseId = courseId,
            UserId = currentUserId,
            Role = EnrollmentRole.Student
        };
        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();

        return _mapper.Map<EnrollmentResponse>(enrollment);
    }

    public async Task<bool> UnenrollCourseAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;

        var course = await db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == courseId);
        if (course is null || course.CreatorId == currentUserId)
            return false;

        var count = await db.Enrollments
            .Where(e => e.CourseId == courseId && e.UserId == currentUserId)
            .ExecuteDeleteAsync();

        return count > 0;
    }

    public async Task<EnrollmentResponse?> SetTAAsync(long enrollmentId, bool enable)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;

        var enrollment = await db.Enrollments.FirstOrDefaultAsync(e => e.Id == enrollmentId && e.Course.CreatorId == currentUserId);
        if (enrollment is null)
            return null;

        enrollment.Role = enable ? EnrollmentRole.TA : EnrollmentRole.Student;
        db.Enrollments.Update(enrollment);
        await db.SaveChangesAsync();

        return _mapper.Map<EnrollmentResponse>(enrollment);
    }

    public async Task<bool> RemoveEnrollmentAsync(long enrollmentId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;

        var count = await db.Enrollments
            .Where(e => e.Id == enrollmentId && e.Course.CreatorId == currentUserId)
            .ExecuteDeleteAsync();

        return count > 0;
    }

    private async Task<List<EnrollmentResponse>> GetEnrollmentsAsync(
        AppDbContext db,
        System.Linq.Expressions.Expression<Func<Enrollment, bool>> predicate)
    {
        return await db.Enrollments
            .AsNoTracking()
            .Where(predicate)
            .Select(e => _mapper.Map<EnrollmentResponse>(e))
            .ToListAsync();
    }
}
