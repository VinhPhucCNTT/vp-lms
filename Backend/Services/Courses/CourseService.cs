
using Backend.Data;
using Backend.Core.Common;
using Microsoft.EntityFrameworkCore;
using Backend.Services.Common;
using Backend.Core.Types;
using Backend.Core.Entities.Courses;
using Sqids;
using AutoMapper;

namespace Backend.Services.Courses;

public class CourseService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper,
    SqidsEncoder<long> sqidsEncoder)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;
    private readonly SqidsEncoder<long> _sqidsEncoder = sqidsEncoder;


    public async Task<CourseDetailResponse?> GetCourseByIdAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new CourseDetailResponse(
                _sqidsEncoder.Encode(c.CreatorId),
                _mapper.Map<UserResponse>(c.Creator),
                c.Title,
                c.Description,
                c.ThumbnailUrl,
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

        if (query.EnrollmentOpen != null)
            courses = courses.Where(c => c.EnrollmentOpen == query.EnrollmentOpen);

        var list = await courses
            .OrderBy(c => c.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(c => c.Creator)
            .Select(c => _mapper.Map<CourseResponse>(c))
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
            .IgnoreQueryFilters()
            .Where(c => c.CreatorId == currentUserId && !c.IsPublished)
            .Include(c => c.Creator)
            .Select(c => _mapper.Map<CourseResponse>(c))
            .ToListAsync();
    }

    public async Task<List<CourseResponse>> GetPublishedCoursesAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Courses
            .AsNoTracking()
            .Include(c => c.Creator)
            .Select(c => _mapper.Map<CourseResponse>(c))
            .ToListAsync();
    }

    public async Task<List<CourseResponse>> GetUserCoursesAsync(long userId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Courses
            .AsNoTracking()
            .Where(c => c.CreatorId == userId)
            .Include(c => c.Creator)
            .Select(c => _mapper.Map<CourseResponse>(c))
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
            EnrollmentOpen = request.EnrollmentOpen
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();
        return _mapper.Map<CourseSetResponse>(course);
    }

    public async Task<CourseSetResponse?> UpdateCourseAsync(long courseId, CourseSetRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId);
        if (course is null)
            return null;

        course.Title = request.Title;
        course.Description = request.Description;
        course.ThumbnailUrl = request.ThumbnailUrl;
        course.IsPublished = request.IsPublished;
        course.EnrollmentOpen = request.EnrollmentOpen;

        db.Courses.Update(course);
        await db.SaveChangesAsync();
        return _mapper.Map<CourseSetResponse>(course);
    }

    public async Task<bool> DeleteCourseAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        var course = await db.Courses.FirstOrDefaultAsync(c => c.Id == courseId && c.CreatorId == currentUserId);
        if (course is null)
            return false;
        db.Courses.Remove(course);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SetCoursePublishStatusAsync(long courseId, bool IsPublished)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        var count = await db.Courses
            .IgnoreQueryFilters()
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
