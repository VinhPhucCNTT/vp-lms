using Backend.Data;
using Backend.Core.Common;
using Microsoft.EntityFrameworkCore;
using Backend.Services.Common;
using Backend.Core.Types;
using Backend.Core.Entities.Courses;
using AutoMapper;

namespace Backend.Services.Courses;

public class CourseService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;

    public async Task<CourseResponse?> GetCourseByIdAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => _mapper.Map<CourseResponse>(c))
            .FirstOrDefaultAsync();
    }

    public async Task<List<CourseStudentResponse>> GetStudentCoursesAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        var enrolledCourseIds = await db.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == currentUserId)
            .Select(e => e.CourseId)
            .ToListAsync();

        var courses = await db.Courses
            .AsNoTracking()
            .Where(c => enrolledCourseIds.Contains(c.Id))
            .ToListAsync();

        List<CourseStudentResponse> list = [];
        foreach (var course in courses)
        {
            var item = new CourseStudentResponse(
                _mapper.Map<CourseResponse>(course),
                await ResourceService.GetCourseProgressAsync(db, course.Id, currentUserId));

            list.Add(item);
        }

        return list;
    }

    public async Task<List<CourseResponse>> GetInstructorCoursesAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        var enrolledCourseIds = await db.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == currentUserId)
            .Select(e => e.CourseId)
            .ToListAsync();

        return await db.Courses
            .AsNoTracking()
            .Where(c => enrolledCourseIds.Contains(c.Id))
            .Select(c => _mapper.Map<CourseResponse>(c))
            .ToListAsync();
    }

    public async Task<CourseExploreResponse> GetExploreAsync()
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var featuredCourses = await GetFeaturedCoursesAsync(db);

        var departments = db.Departments.AsNoTracking();
        Dictionary<string, List<CourseResponse>> coursesByDepartment = [];
        foreach (var department in departments)
        {
            var courses = await db.Courses
                .AsNoTracking()
                .Where(c => c.DepartmentId == department.Id)
                .Select(c => _mapper.Map<CourseResponse>(c))
                .ToListAsync();

            coursesByDepartment.Add(department.Name, courses);
        }

        var recentlyUpdated = await db.Courses
            .AsNoTracking()
            .OrderByDescending(c => c.UpdatedAt)
            .Take(10)
            .Select(c => _mapper.Map<CourseResponse>(c))
            .ToListAsync();

        return new CourseExploreResponse(featuredCourses, coursesByDepartment, recentlyUpdated);
    }

    public async Task<QueryResponse<CourseResponse>> QueryCoursesAsync(CourseRequest query)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var courses = db.Courses.AsNoTracking().Where(c => c.IsPublished);

        if (!string.IsNullOrEmpty(query.Code))
            courses = courses.Where(c => c.Code.Contains(query.Code, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.Title))
            courses = courses.Where(c => c.Title.Contains(query.Title, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.CreatorUsername))
            courses = courses.Where(c => c.Creator.Username.Contains(query.CreatorUsername, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.CreatorFullname))
            courses = courses.Where(c => c.Creator.Fullname.Contains(query.CreatorFullname, StringComparison.OrdinalIgnoreCase));

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

    public async Task<bool> SetCoursePublishStatusAsync(long courseId, bool value)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        var count = await db.Courses
            .Where(c => c.Id == courseId && c.CreatorId == currentUserId)
            .Where(c => c.IsPublished != value)
            .ExecuteUpdateAsync(c => c.SetProperty(c => c.IsPublished, value));

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

    // Placeholder
    private async Task<List<CourseResponse>> GetFeaturedCoursesAsync(AppDbContext db)
    {
        return await db.Courses
            .AsNoTracking()
            .OrderByDescending(c => c.Title)
            .Take(10)
            .Select(c => _mapper.Map<CourseResponse>(c))
            .ToListAsync();
    }

    public async Task<CourseAuthorizationResource?> GetAuthorizationResourceAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new CourseAuthorizationResource(c.Id, c.CreatorId))
            .FirstOrDefaultAsync();
    }
}
