
using Backend.Data;
using Backend.Core.Common;
using Microsoft.EntityFrameworkCore;
using Backend.Services.Common;
using Backend.Core.Types;
using Backend.Core.Entities.Courses;
using Sqids;

namespace Backend.Services.Courses;

public class CourseService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    SqidsEncoder<long> sqidsEncoder
)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly SqidsEncoder<long> _sqidsEncoder = sqidsEncoder;

    public async Task<CourseDetailResponse?> GetCourseByIdAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new CourseDetailResponse(
                _sqidsEncoder.Encode(c.CreatorId),
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
        var courses = db.Courses.AsNoTracking().Where(c => c.IsPublished);

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
                _sqidsEncoder.Encode(c.CreatorId),
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

    // Get all unpublished course belonging to the current user
    // TODO: May have to reconsider this once course versioning is fully fleshed out
    public async Task<List<CourseResponse>> GetUnpublishedCoursesAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.Courses
            .AsNoTracking()
            .Where(c => c.CreatorId == currentUserId && !c.IsPublished)
            .Select(c => new CourseResponse(
                _sqidsEncoder.Encode(c.CreatorId),
                c.Creator.Username,
                c.Title,
                c.ThumbnailUrl,
                c.AllowAnonymousAccess,
                c.EnrollmentOpen))
            .ToListAsync();
    }

    public async Task<List<CourseResponse>> GetPublishedCoursesAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Courses
            .AsNoTracking()
            .Select(c => new CourseResponse(
                _sqidsEncoder.Encode(c.CreatorId),
                c.Creator.Username,
                c.Title,
                c.ThumbnailUrl,
                c.AllowAnonymousAccess,
                c.EnrollmentOpen))
            .ToListAsync();
    }

    public async Task<List<CourseResponse>> GetUserCoursesAsync(long userId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Courses
            .AsNoTracking()
            .Where(c => c.CreatorId == userId)
            .Select(c => new CourseResponse(
                _sqidsEncoder.Encode(c.CreatorId),
                c.Creator.Username,
                c.Title,
                c.ThumbnailUrl,
                c.AllowAnonymousAccess,
                c.EnrollmentOpen))
            .ToListAsync();
    }

    public async Task<CourseSetResponse> CreateCourseAsync(CourseSetRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = new Course
        {
            CreatorId = _currentUserService.UserId,
            Title = request.Title,
            Description = request.Description,
            ThumbnailUrl = request.ThumbnailUrl,
            IsPublished = request.IsPublished,
            AllowAnonymousAccess = request.AllowAnonymousAccess,
            EnrollmentOpen = request.EnrollmentOpen
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();
        return new CourseSetResponse(
            _sqidsEncoder.Encode(course.Id),
            _sqidsEncoder.Encode(course.CreatorId),
            course.Title,
            course.Description,
            course.ThumbnailUrl,
            course.IsPublished,
            course.AllowAnonymousAccess,
            course.EnrollmentOpen
        );
    }

    public async Task<CourseSetResponse?> UpdateCourseAsync(long courseId, CourseSetRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course == null)
            return null;

        course.Title = request.Title;
        course.Description = request.Description;
        course.ThumbnailUrl = request.ThumbnailUrl;
        course.IsPublished = request.IsPublished;
        course.AllowAnonymousAccess = request.AllowAnonymousAccess;
        course.EnrollmentOpen = request.EnrollmentOpen;

        db.Courses.Update(course);
        await db.SaveChangesAsync();
        return new CourseSetResponse(
            _sqidsEncoder.Encode(course.Id),
            _sqidsEncoder.Encode(course.CreatorId),
            course.Title,
            course.Description,
            course.ThumbnailUrl,
            course.IsPublished,
            course.AllowAnonymousAccess,
            course.EnrollmentOpen
        );
    }

    public async Task<bool> DeleteCourseAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        var count = await db.Courses
            .Where(c => c.Id == courseId && c.CreatorId == currentUserId)
            .ExecuteDeleteAsync();

        return count > 0;
    }

    public async Task<bool> SetCoursePublishStatusAsync(long courseId, bool IsPublished)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        var count = await db.Courses
            .Where(c => c.Id == courseId && c.CreatorId == currentUserId)
            .Where(c => c.IsPublished == IsPublished)
            .ExecuteUpdateAsync(c => c.SetProperty(c => c.IsPublished, IsPublished));

        return count > 0;
    }

    public async Task<bool> CheckOwnerAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId && c.CreatorId == currentUserId)
            .AnyAsync();
    }
}
