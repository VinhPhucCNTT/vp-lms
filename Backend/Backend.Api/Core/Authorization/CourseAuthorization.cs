using Backend.Api.Services.Common;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Courses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Authorization;

public interface ICourseAuthorization
{
    public Task<bool> IsCourseOwnerAsync(Course course);

    public Task<bool> IsParticipantAsync(long courseId, CancellationToken ct = default);
}

public class CourseAuthorization(
    CurrentUserService currentUserService,
    IDbContextFactory<AppDbContext> dbFactory) : ICourseAuthorization
{
    private readonly CurrentUserService currentUserService = currentUserService;
    private readonly IDbContextFactory<AppDbContext> dbFactory = dbFactory;

    public async Task<bool> IsCourseOwnerAsync(Course course)
    {
        var userId = currentUserService.UserId;
        return course.CreatorId == userId;
    }

    public async Task<bool> IsParticipantAsync(long courseId, CancellationToken ct = default)
    {
        await using var db = await dbFactory.CreateDbContextAsync(ct);
        var userId = currentUserService.UserId;

        return await db.Enrollments.AsNoTracking().AnyAsync(e => e.CourseId == courseId && e.UserId == userId, ct);
    }
}
