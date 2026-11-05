using AutoMapper;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Common;
using Backend.Api.Services.Content;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Courses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Courses;

public interface ICourseService
{
    public Task<Course?> GetAsync(long courseId, CancellationToken ct = default);

    public Task<Course?> GetFromModuleAsync(long moduleId, CancellationToken ct = default);

    public Task<CourseResponse?> GetDtoAsync(long courseId, CancellationToken ct = default);

    public Task<List<CourseStudentResponse>> GetStudentCoursesAsync(CancellationToken ct = default);

    public Task<List<CourseResponse>> GetInstructorCoursesAsync(CancellationToken ct = default);

    public Task<CourseExploreResponse> GetExploreAsync(CancellationToken ct = default);

    public Task<PaginatedResponse<CourseResponse>> QueryCoursesAsync(CourseRequest query, CancellationToken ct = default);

    public Task<long> UploadBackgroundAsync(
        Course course,
        IFormFile file,
        CancellationToken ct = default);

    public Task<CourseSetResponse> CreateCourseAsync(CourseSetRequest request, CancellationToken ct = default);

    public Task<CourseSetResponse> UpdateCourseAsync(Course course, CourseSetRequest request, CancellationToken ct = default);

    public Task DeleteCourseAsync(Course course, CancellationToken ct = default);

    public Task SetCoursePublishStatusAsync(Course course, bool value, CancellationToken ct = default);

    public Task<List<RecentCourseResponse>> GetRecentAsync(int count, CancellationToken ct = default);
}

public class CourseService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IFileService fileService,
    IMapper mapper) : ICourseService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IFileService _fileService = fileService;
    private readonly IMapper _mapper = mapper;

    public async Task<Course?> GetAsync(long courseId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.Courses
            .Where(c => c.Id == courseId)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<Course?> GetFromModuleAsync(long moduleId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.CourseModules
            .Where(m => m.Id == moduleId)
            .Select(m => m.Course)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<CourseResponse?> GetDtoAsync(long courseId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var course = await db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .FirstOrDefaultAsync(ct);
        if (course is null)
            return null;

        await SetRecentCourseAsync(db, course, _currentUserService.UserId, ct);

        return _mapper.Map<CourseResponse>(course);
    }

    public async Task<List<CourseStudentResponse>> GetStudentCoursesAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var currentUserId = _currentUserService.UserId;
        var enrolledCourseIds = await db.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == currentUserId)
            .Select(e => e.CourseId)
            .ToListAsync(ct);

        var courses = await db.Courses
            .AsNoTracking()
            .Where(c => enrolledCourseIds.Contains(c.Id))
            .ToListAsync(ct);

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

    public async Task<List<CourseResponse>> GetInstructorCoursesAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var currentUserId = _currentUserService.UserId;

        return await db.Courses
            .AsNoTracking()
            .Where(c => c.CreatorId == currentUserId)
            .Select(c => _mapper.Map<CourseResponse>(c))
            .ToListAsync(ct);
    }

    public async Task<CourseExploreResponse> GetExploreAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var featuredCourses = await GetFeaturedCoursesAsync(db, ct);

        var departments = db.Departments.AsNoTracking();
        Dictionary<string, List<CourseResponse>> coursesByDepartment = [];
        foreach (var department in departments)
        {
            var courses = await db.Courses
                .AsNoTracking()
                .Where(c => c.DepartmentId == department.Id)
                .Select(c => _mapper.Map<CourseResponse>(c))
                .ToListAsync(ct);

            coursesByDepartment.Add(department.Name, courses);
        }

        var recentlyUpdated = await db.Courses
            .AsNoTracking()
            .OrderByDescending(c => c.UpdatedAt)
            .Take(10)
            .Select(c => _mapper.Map<CourseResponse>(c))
            .ToListAsync(ct);

        return new CourseExploreResponse(featuredCourses, coursesByDepartment, recentlyUpdated);
    }

    public async Task<PaginatedResponse<CourseResponse>> QueryCoursesAsync(CourseRequest query, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
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
            .ToListAsync(ct);

        return new PaginatedResponse<CourseResponse>(
                query.PageNumber,
                query.PageSize,
                await courses.CountAsync(ct),
                list);
    }

    public async Task<long> UploadBackgroundAsync(
        Course course,
        IFormFile file,
        CancellationToken ct)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        // TODO: Validation
        await using var stream = file.OpenReadStream();
        long fileId = await _fileService.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            _currentUserService.UserId,
            FileCategory.CourseBackground,
            ct);

        if (course.BackgroundFileId is not null)
            await _fileService.DeleteAsync((long)course.BackgroundFileId, ct);

        course.BackgroundFileId = fileId;
        await db.SaveChangesAsync(ct);

        return fileId;
    }

    public async Task<CourseSetResponse> CreateCourseAsync(CourseSetRequest request, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var course = new Course
        {
            CreatorId = _currentUserService.UserId,
            Code = request.Code,
            Title = request.Title,
            Description = request.Description,
            IsPublished = request.IsPublished,
            EnrollmentOpen = request.EnrollmentOpen
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync(ct);
        return _mapper.Map<CourseSetResponse>(course);
    }

    public async Task<CourseSetResponse> UpdateCourseAsync(Course course, CourseSetRequest request, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        course.Title = request.Title;
        course.Description = request.Description;
        course.IsPublished = request.IsPublished;
        course.EnrollmentOpen = request.EnrollmentOpen;

        db.Courses.Update(course);
        await db.SaveChangesAsync(ct);
        return _mapper.Map<CourseSetResponse>(course);
    }

    public async Task DeleteCourseAsync(Course course, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        db.Courses.Remove(course);
        await db.SaveChangesAsync(ct);
        return;
    }

    public async Task SetCoursePublishStatusAsync(Course course, bool value, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        course.IsPublished = value;
        await db.SaveChangesAsync(ct);
        return;
    }

    public async Task<List<RecentCourseResponse>> GetRecentAsync(int count, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var currentUserId = _currentUserService.UserId;

        var records = db.RecentCourses
            .AsNoTracking()
            .OrderByDescending(r => r.LastAccessedAt)
            .Where(r => r.UserId == currentUserId)
            .Select(r => _mapper.Map<RecentCourseResponse>(r));

        if (count > 0)
            records = records.Take(count);

        return await records.ToListAsync(ct);
    }

    public async Task SetRecentCourseAsync(AppDbContext db, Course course, long userId, CancellationToken ct = default)
    {
        var recent = await db.RecentCourses.FirstOrDefaultAsync(r => r.CourseId == course.Id && r.UserId == userId, ct);

        if (recent is null)
        {
            recent = new RecentCourse
            {
                UserId = userId,
                CourseId = course.Id,

                LastAccessedAt = DateTime.UtcNow
            };

            db.RecentCourses.Add(recent);
        }
        else
        {
            recent.LastAccessedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(ct);
        return;
    }

    // Placeholder
    private async Task<List<CourseResponse>> GetFeaturedCoursesAsync(AppDbContext db, CancellationToken ct = default)
    {
        return await db.Courses
            .AsNoTracking()
            .OrderByDescending(c => c.Title)
            .Take(10)
            .Select(c => _mapper.Map<CourseResponse>(c))
            .ToListAsync(ct);
    }

}
