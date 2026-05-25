
using Backend.Data;
using Backend.Common;
using Backend.Models.Courses;
using Backend.Features.Courses.Types;
using Microsoft.EntityFrameworkCore;

namespace Backend.Features.Courses.Services;

public class CCourseService(
    IDbContextFactory<AppDbContext> dbFactory
) : ICCourseService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

    public async Task<MResult<CourseSetResponse, CourseSetError>> CreateCourseAsync(Guid userId, CourseSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (!await db.Users.AnyAsync(u => u.Id == userId))
            return MResult<CourseSetResponse, CourseSetError>
                .Failure(CourseSetError.InvalidRequest);

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

        return MResult<CourseSetResponse, CourseSetError>
            .Success(new CourseSetResponse(
                course.Title,
                course.Description,
                course.ThumbnailUrl,
                course.IsPublished,
                course.AllowAnonymousAccess,
                course.EnrollmentOpen
            ));
    }

    public async Task<MResult<CourseSetResponse, CourseSetError>> UpdateCourseAsync(Guid userId, Guid courseId, CourseSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return MResult<CourseSetResponse, CourseSetError>
                .Failure(CourseSetError.InvalidRequest);
        if (course.CreatorId != userId)
            return MResult<CourseSetResponse, CourseSetError>
                .Failure(CourseSetError.Unauthorized);

        course.Title = dto.Title;
        course.Description = dto.Description;
        course.ThumbnailUrl = dto.ThumbnailUrl;
        course.IsPublished = dto.IsPublished;
        course.AllowAnonymousAccess = dto.AllowAnonymousAccess;
        course.EnrollmentOpen = dto.EnrollmentOpen;

        db.Courses.Update(course);
        await db.SaveChangesAsync();

        return MResult<CourseSetResponse, CourseSetError>
            .Success(new CourseSetResponse(
                course.Title,
                course.Description,
                course.ThumbnailUrl,
                course.IsPublished,
                course.AllowAnonymousAccess,
                course.EnrollmentOpen
            ));
    }

    public async Task<CourseDeleteStatus> DeleteCourseAsync(Guid userId, Guid courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return CourseDeleteStatus.InvalidRequest;
        if (course.CreatorId != userId)
            return CourseDeleteStatus.Unauthorized;

        db.Courses.Remove(course);
        await db.SaveChangesAsync();

        return CourseDeleteStatus.Success;
    }
}
