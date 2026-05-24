
using Backend.Data;
using Backend.Common;
using Backend.Models.Courses;
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

    public async Task<CourseSetResult> CreateCourseAsync(Guid userId, CourseSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return new CourseSetResult { Response = null, Status = CourseSetStatus.InvalidUser };

        var course = new Course
        {
            CreatorId = userId,
            Title = dto.Title,
            Description = dto.Description,
            ThumbnailUrl = dto.ThumbnailUrl,
            IsPublished = dto.IsPublished,
            AllowAnonymousAccess = dto.AllowAnonymousAccess,
            EnrollmentOpen = dto.EnrollmentOpen
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();

        var response = new CourseSetResponse(
            course.Title,
            course.Description,
            course.ThumbnailUrl,
            course.IsPublished,
            course.AllowAnonymousAccess,
            course.EnrollmentOpen
        );

        return new CourseSetResult { Response = response, Status = CourseSetStatus.Success };
    }

    public async Task<CourseSetResult> UpdateCourseAsync(Guid userId, Guid courseId, CourseSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return new CourseSetResult { Response = null, Status = CourseSetStatus.InvalidCourse };
        if (course.CreatorId != userId)
            return new CourseSetResult { Response = null, Status = CourseSetStatus.InvalidUser };

        course.Title = dto.Title;
        course.Description = dto.Description;
        course.ThumbnailUrl = dto.ThumbnailUrl;
        course.IsPublished = dto.IsPublished;
        course.AllowAnonymousAccess = dto.AllowAnonymousAccess;
        course.EnrollmentOpen = dto.EnrollmentOpen;

        db.Courses.Update(course);
        await db.SaveChangesAsync();

        var response = new CourseSetResponse(
            course.Title,
            course.Description,
            course.ThumbnailUrl,
            course.IsPublished,
            course.AllowAnonymousAccess,
            course.EnrollmentOpen
        );

        return new CourseSetResult { Response = response, Status = CourseSetStatus.Success };
    }

    public async Task<CourseDeleteStatus> DeleteCourseAsync(Guid userId, Guid courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return CourseDeleteStatus.InvalidCourse;
        if (course.CreatorId != userId)
            return CourseDeleteStatus.InvalidUser;

        db.Courses.Remove(course);
        await db.SaveChangesAsync();

        return CourseDeleteStatus.Success;
    }
}
