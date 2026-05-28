
using Backend.Data;
using Backend.Core.Common;
using Microsoft.EntityFrameworkCore;
using Backend.Services.Common;
using Backend.Core.Types;
using Backend.Core.Entities.Courses;

namespace Backend.Services.Courses;

public class CourseService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService
)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;

    public async Task<CourseDetailResponse?> GetCourseByIdAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new CourseDetailResponse(
                c.CreatorId,
                UserResponse.Set(c.Creator),
                c.Title,
                c.Description,
                c.ThumbnailUrl,
                c.AllowAnonymousAccess,
                c.EnrollmentOpen))
            .FirstOrDefaultAsync();
    }

    public async Task<QueryResponse<CourseResponse>> QueryCoursesAsync(CourseRequest query)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var courses = db.Courses.AsNoTracking();

        if (!string.IsNullOrEmpty(query.Title))
            courses = courses.Where(c => c.Title.Contains(query.Title, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.CreatorUserName))
            courses = courses.Where(c => c.Creator.Username.Contains(query.CreatorUserName, StringComparison.OrdinalIgnoreCase));

        if (query.AllowAnonymousAccess != null)
            courses = courses.Where(c => c.AllowAnonymousAccess == query.AllowAnonymousAccess);

        if (query.EnrollmentOpen != null)
            courses = courses.Where(c => c.EnrollmentOpen == query.EnrollmentOpen);

        var list = await courses
            .OrderBy(c => c.Id)
            .Select(c => new CourseResponse(
                c.CreatorId,
                c.Creator.Username,
                c.Title,
                c.ThumbnailUrl,
                c.AllowAnonymousAccess,
                c.EnrollmentOpen))
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return new QueryResponse<CourseResponse>(
                query.PageNumber,
                query.PageSize,
                await courses.CountAsync(),
                list);
    }

    public async Task<bool> CreateCourseAsync(CourseSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = new Course
        {
            CreatorId = _currentUserService.UserId,
            Title = dto.Title,
            Description = dto.Description,
            ThumbnailUrl = dto.ThumbnailUrl,
            IsPublished = dto.IsPublished,
            AllowAnonymousAccess = dto.AllowAnonymousAccess,
            EnrollmentOpen = dto.EnrollmentOpen
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCourseAsync(long courseId, CourseSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null || course.CreatorId != _currentUserService.UserId)
            return false;

        course.Title = dto.Title;
        course.Description = dto.Description;
        course.ThumbnailUrl = dto.ThumbnailUrl;
        course.IsPublished = dto.IsPublished;
        course.AllowAnonymousAccess = dto.AllowAnonymousAccess;
        course.EnrollmentOpen = dto.EnrollmentOpen;

        db.Courses.Update(course);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteCourseAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null || course.CreatorId != _currentUserService.UserId)
            return false;

        db.Courses.Remove(course);
        await db.SaveChangesAsync();

        return true;
    }
}
