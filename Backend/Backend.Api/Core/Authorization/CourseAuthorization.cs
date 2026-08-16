using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Data;
using Backend.Api.Services.Common;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Core.Authorization;

public class CourseAuthorization(
    CurrentUserService currentUserService,
    IDbContextFactory<AppDbContext> dbFactory)
{
    private readonly CurrentUserService currentUserService = currentUserService;
    private readonly IDbContextFactory<AppDbContext> dbFactory = dbFactory;

    public async Task<bool> IsCourseOwnerAsync(Course course)
    {
        var userId = currentUserService.UserId;
        return course.CreatorId == userId;
    }

    public async Task<bool> IsParticipantAsync(long courseId)
    {
        using var db = await dbFactory.CreateDbContextAsync();
        var userId = currentUserService.UserId;

        return await db.Enrollments.AsNoTracking().AnyAsync(e => e.CourseId == courseId && e.UserId == userId);
    }
}
