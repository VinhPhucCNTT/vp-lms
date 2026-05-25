
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

    public async Task<MResult<CCourseResponse, CCourseGetError>> GetCourseByIdAsync(Guid userId, Guid courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = await db.Courses.Include(c => c.Creator).FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return MResult<CCourseResponse, CCourseGetError>
                .Failure(CCourseGetError.NotFound);
        if (course.CreatorId != userId)
            return MResult<CCourseResponse, CCourseGetError>
                .Failure(CCourseGetError.Unauthorized);

        return MResult<CCourseResponse, CCourseGetError>
            .Success(new CCourseResponse(
                course.CreatorId,
                course.Creator.Username,
                course.Title,
                course.Description,
                course.ThumbnailUrl,
                course.AllowAnonymousAccess,
                course.EnrollmentOpen,
                course.IsPublished
            ));
    }

    public async Task<QueryResponse<CCourseResponse>> QueryCoursesAsync(Guid userId, CCourseRequest query)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var courses = db.Courses.Include(c => c.Creator).Where(c => c.CreatorId == userId);

        if (!await courses.AnyAsync())
            return new QueryResponse<CCourseResponse>(query.PageNumber, query.PageSize, 0, []);

        if (!string.IsNullOrEmpty(query.Title))
            courses = courses.Where(c => c.Title.Equals(query.Title, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.CreatorUserName))
            courses = courses.Where(c => c.Creator.Username.Equals(query.CreatorUserName, StringComparison.OrdinalIgnoreCase));

        if (query.AllowAnonymousAccess != null)
            courses = courses.Where(c => c.AllowAnonymousAccess == query.AllowAnonymousAccess);

        if (query.EnrollmentOpen != null)
            courses = courses.Where(c => c.EnrollmentOpen == query.EnrollmentOpen);

        if (query.IsPublished != null)
            courses = courses.Where(c => c.IsPublished == query.IsPublished);

        var username = courses.First().Creator.Username;
        var list = await courses
            .Select(c => new CCourseResponse(
                c.Id,
                username,
                c.Title,
                c.Description,
                c.ThumbnailUrl,
                c.AllowAnonymousAccess,
                c.EnrollmentOpen,
                c.IsPublished))
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

            return new QueryResponse<CCourseResponse>(query.PageNumber, query.PageSize, await courses.CountAsync(), list);
    }

    public async Task<MResult<CCourseSetResponse, CCourseSetError>> CreateCourseAsync(Guid userId, CCourseSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        if (!await db.Users.AnyAsync(u => u.Id == userId))
            return MResult<CCourseSetResponse, CCourseSetError>
                .Failure(CCourseSetError.InvalidRequest);

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

        return MResult<CCourseSetResponse, CCourseSetError>
            .Success(new CCourseSetResponse(
                course.Title,
                course.Description,
                course.ThumbnailUrl,
                course.IsPublished,
                course.AllowAnonymousAccess,
                course.EnrollmentOpen
            ));
    }

    public async Task<MResult<CCourseSetResponse, CCourseSetError>> UpdateCourseAsync(Guid userId, Guid courseId, CCourseSetRequest dto)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return MResult<CCourseSetResponse, CCourseSetError>
                .Failure(CCourseSetError.InvalidRequest);
        if (course.CreatorId != userId)
            return MResult<CCourseSetResponse, CCourseSetError>
                .Failure(CCourseSetError.Unauthorized);

        course.Title = dto.Title;
        course.Description = dto.Description;
        course.ThumbnailUrl = dto.ThumbnailUrl;
        course.IsPublished = dto.IsPublished;
        course.AllowAnonymousAccess = dto.AllowAnonymousAccess;
        course.EnrollmentOpen = dto.EnrollmentOpen;

        db.Courses.Update(course);
        await db.SaveChangesAsync();

        return MResult<CCourseSetResponse, CCourseSetError>
            .Success(new CCourseSetResponse(
                course.Title,
                course.Description,
                course.ThumbnailUrl,
                course.IsPublished,
                course.AllowAnonymousAccess,
                course.EnrollmentOpen
            ));
    }

    public async Task<CCourseDeleteStatus> DeleteCourseAsync(Guid userId, Guid courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return CCourseDeleteStatus.InvalidRequest;
        if (course.CreatorId != userId)
            return CCourseDeleteStatus.Unauthorized;

        db.Courses.Remove(course);
        await db.SaveChangesAsync();

        return CCourseDeleteStatus.Success;
    }
}
