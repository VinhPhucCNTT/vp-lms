
using Backend.Data;
using Backend.Common;
using Backend.Features.Courses.Types;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Courses.Services;

public class CourseService(
    IDbContextFactory<AppDbContext> dbFactory
) : ICourseService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

    public async Task<CourseResponse?> GetCourseByIdAsync(Guid courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = await db.Courses.Include(c => c.Creator).FirstOrDefaultAsync(c => c.Id == courseId);

        return (course == null) ? null : new CourseResponse(
            course.CreatorId,
            course.Creator.Username,
            course.Title,
            course.Description,
            course.ThumbnailUrl,
            course.AllowAnonymousAccess,
            course.EnrollmentOpen
        );
    }

    public async Task<QueryResponse<CourseResponse>> QueryCoursesAsync(CourseRequest query)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var courses = db.Courses.Include(c => c.Creator).AsQueryable();

        if (!string.IsNullOrEmpty(query.Title))
            courses = courses.Where(c => c.Title.Equals(query.Title, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.CreatorUserName))
            courses = courses.Where(c => c.Creator.Username.Equals(query.CreatorUserName, StringComparison.OrdinalIgnoreCase));

        if (query.AllowAnonymousAccess != null)
            courses = courses.Where(c => c.AllowAnonymousAccess == query.AllowAnonymousAccess);

        if (query.EnrollmentOpen != null)
            courses = courses.Where(c => c.EnrollmentOpen == query.EnrollmentOpen);

        var list = await courses
            .Select(c => new CourseResponse(
                c.CreatorId,
                c.Creator.Username,
                c.Title,
                c.Description,
                c.ThumbnailUrl,
                c.AllowAnonymousAccess,
                c.EnrollmentOpen))
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new QueryResponse<CourseResponse>(query.PageNumber, query.PageSize, await courses.CountAsync(), list);
    }
}
