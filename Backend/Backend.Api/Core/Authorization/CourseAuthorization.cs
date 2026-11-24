using Backend.Api.Services.Common;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Users;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Authorization;

public interface ICourseAuthorization
{
    public Task<CourseReadAccess?> GetReadAccessAsync(long courseId, CancellationToken ct = default);

    public Task<bool> IsCourseOwnerAsync(Course course);

    public Task<bool> IsParticipantAsync(long courseId, CancellationToken ct = default);
}

public sealed record CourseReadAccess(bool IsOwner);

public class CourseAuthorization(
    CurrentUserService currentUserService,
    IDbContextFactory<AppDbContext> dbFactory) : ICourseAuthorization
{
    private readonly CurrentUserService currentUserService = currentUserService;
    private readonly IDbContextFactory<AppDbContext> dbFactory = dbFactory;

    // Used by the course/module/activity read routes only. Publication never
    // substitutes for enrollment, and enrollment-open does not govern reading.
    public async Task<CourseReadAccess?> GetReadAccessAsync(long courseId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var userId = currentUserService.UserId;
        var principal = currentUserService.Principal;
        var isStudent = principal?.IsInRole(nameof(UserRoles.Student)) == true;
        var canOwn = principal?.IsInRole(nameof(UserRoles.Instructor)) == true ||
            principal?.IsInRole(nameof(UserRoles.Admin)) == true;

        return await db.Courses.AsNoTracking()
            .Where(c => c.Id == courseId && !c.IsDeleted)
            .Where(c => (canOwn && c.InstructorId == userId) ||
                (isStudent && c.IsPublished && db.Enrollments.Any(e =>
                    e.CourseId == c.Id && e.UserId == userId && !e.IsDeleted)))
            .Select(c => new CourseReadAccess(canOwn && c.InstructorId == userId))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> IsCourseOwnerAsync(Course course)
    {
        var userId = currentUserService.UserId;
        return course.InstructorId == userId;
    }

    public async Task<bool> IsParticipantAsync(long courseId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var userId = currentUserService.UserId;

        return await db.Enrollments.AsNoTracking().AnyAsync(e => e.CourseId == courseId && e.UserId == userId, ct);
    }
}
