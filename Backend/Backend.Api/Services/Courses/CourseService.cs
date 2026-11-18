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

    public Task<CourseDetailResponse?> GetDtoAsync(long courseId, CancellationToken ct = default);

    public Task<PaginatedResponse<StudentCourseResponse>> GetStudentCoursesAsync(PageRequest page, CancellationToken ct = default);

    public Task<PaginatedResponse<InstructorCourseResponse>> GetInstructorCoursesAsync(PageRequest page, CancellationToken ct = default);

    public Task<ExploreCourseResponse> GetExploreAsync(CancellationToken ct = default);

    public Task<PaginatedResponse<CourseDetailResponse>> QueryCoursesAsync(CourseQueryRequest query, CancellationToken ct = default);

    public Task<CourseSetResponse> CreateCourseAsync(CourseCreateRequest request, CancellationToken ct = default);

    public Task<CourseSetResponse?> UpdateCourseAsync(Course course, CoursePatchRequest request, CancellationToken ct = default);

    public Task DeleteCourseAsync(Course course, CancellationToken ct = default);

    public Task SetCoursePublishStatusAsync(Course course, bool value, CancellationToken ct = default);

    public Task<List<CourseRecentResponse>> GetRecentAsync(int count, CancellationToken ct = default);
}

public class CourseService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IFileService fileService,
    IMapper mapper,
    ILogger<CourseService> logger) : ICourseService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IFileService _fileService = fileService;
    private readonly IMapper _mapper = mapper;
    private readonly ILogger<CourseService> _logger = logger;

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

    public async Task<CourseDetailResponse?> GetDtoAsync(long courseId, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var course = await db.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .FirstOrDefaultAsync(ct);
        if (course is null)
            return null;

        await SetRecentCourseAsync(db, course, _currentUserService.UserId, ct);

        return _mapper.Map<CourseDetailResponse>(course);
    }

    public async Task<PaginatedResponse<StudentCourseResponse>> GetStudentCoursesAsync(PageRequest page, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var currentUserId = _currentUserService.UserId;

        var courses = db.Courses
            .AsNoTracking()
            .Where(c => db.Enrollments
                .Where(e => e.UserId == currentUserId)
                .Select(e => e.CourseId)
                .Contains(c.Id));

        var list = await courses
            .Skip((page.PageNumber - 1) * page.PageSize)
            .Take(page.PageSize)
            .ToListAsync(ct);

        List<StudentCourseResponse> results = [];
        foreach (var c in list)
        {
            var item = new StudentCourseResponse(
                _mapper.Map<CourseDetailResponse>(c),
                await ActivityService.GetCourseProgressAsync(db, c.Id, currentUserId, ct));

            results.Add(item);
        }

        return new PaginatedResponse<StudentCourseResponse>(
            page.PageNumber,
            page.PageSize,
            await courses.CountAsync(ct),
            results);
    }

    public async Task<PaginatedResponse<InstructorCourseResponse>> GetInstructorCoursesAsync(PageRequest page, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var currentUserId = _currentUserService.UserId;

        var courses = db.Courses
            .AsNoTracking()
            .Where(c => c.InstructorId == currentUserId);

        var list = await courses
            .Skip((page.PageNumber - 1) * page.PageSize)
            .Take(page.PageSize)
            .ToListAsync(ct);

        List<InstructorCourseResponse> results = [];
        foreach (var c in list)
        {
            var item = new InstructorCourseResponse(
                _mapper.Map<CourseDetailResponse>(c),
                c.IsPublished
            );

            results.Add(item);
        }

        return new PaginatedResponse<InstructorCourseResponse>(
            page.PageNumber,
            page.PageSize,
            await courses.CountAsync(ct),
            results);
    }

    public async Task<ExploreCourseResponse> GetExploreAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var featuredCourses = await GetFeaturedCoursesAsync(db, ct);

        var departments = db.Departments.AsNoTracking();
        Dictionary<string, List<CourseDetailResponse>> coursesByDepartment = [];
        foreach (var department in departments)
        {
            var courses = await db.Courses
                .AsNoTracking()
                .Where(c => c.DepartmentId == department.Id)
                .Select(c => _mapper.Map<CourseDetailResponse>(c))
                .ToListAsync(ct);

            coursesByDepartment.Add(department.Name, courses);
        }

        var recentlyUpdated = await db.Courses
            .AsNoTracking()
            .OrderByDescending(c => c.UpdatedAt)
            .Take(10)
            .Select(c => _mapper.Map<CourseDetailResponse>(c))
            .ToListAsync(ct);

        return new ExploreCourseResponse(featuredCourses, coursesByDepartment, recentlyUpdated);
    }

    public async Task<PaginatedResponse<CourseDetailResponse>> QueryCoursesAsync(CourseQueryRequest query, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var courses = db.Courses.AsNoTracking().Where(c => c.IsPublished);

        if (!string.IsNullOrEmpty(query.Title))
            courses = courses.Where(c => c.Title.Contains(query.Title, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.InstructorFirstName))
            courses = courses.Where(c => c.Instructor.FirstName.Contains(query.InstructorFirstName, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrEmpty(query.InstructorLastName))
            courses = courses.Where(c => c.Instructor.LastName.Contains(query.InstructorLastName, StringComparison.OrdinalIgnoreCase));

        if (query.EnrollmentOpen != null)
            courses = courses.Where(c => c.EnrollmentOpen == query.EnrollmentOpen);

        var list = await courses
            .OrderBy(c => c.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(c => c.Instructor)
            .Select(c => _mapper.Map<CourseDetailResponse>(c))
            .ToListAsync(ct);

        return new PaginatedResponse<CourseDetailResponse>(
                query.PageNumber,
                query.PageSize,
                await courses.CountAsync(ct),
                list);
    }

    public async Task<CourseSetResponse> CreateCourseAsync(CourseCreateRequest request, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var course = new Course
        {
            InstructorId = _currentUserService.UserId,
            Title = request.Title,
            Description = request.Description ?? "",
            EnrollmentOpen = request.EnrollmentOpen
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync(ct);
        return _mapper.Map<CourseSetResponse>(course);
    }

    public async Task<CourseSetResponse?> UpdateCourseAsync(
        Course course,
        CoursePatchRequest request,
        CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        await using var transaction = await db.Database.BeginTransactionAsync(ct);

        try
        {
            if (request.Title is not null)
                course.Title = request.Title;

            if (request.Description is not null)
                course.Description = request.Description;

            if (request.EnrollmentOpen is not null)
                course.EnrollmentOpen = request.EnrollmentOpen.Value;

            if (request.BackgroundFile is not null)
                await UploadBackgroundAsync(db, course, request.BackgroundFile, ct);

            db.Courses.Update(course);
            await db.SaveChangesAsync(ct);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occured when updating course {CourseId}.", course.Id);
            await transaction.RollbackAsync(ct);
        }

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

    public async Task<List<CourseRecentResponse>> GetRecentAsync(int count, CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var currentUserId = _currentUserService.UserId;

        var records = db.RecentCourses
            .AsNoTracking()
            .OrderByDescending(r => r.LastAccessedAt)
            .Where(r => r.UserId == currentUserId)
            .Select(r => _mapper.Map<CourseRecentResponse>(r));

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
    private async Task<List<CourseDetailResponse>> GetFeaturedCoursesAsync(AppDbContext db, CancellationToken ct = default)
    {
        return await db.Courses
            .AsNoTracking()
            .OrderByDescending(c => c.Title)
            .Take(10)
            .Select(c => _mapper.Map<CourseDetailResponse>(c))
            .ToListAsync(ct);
    }

    private async Task<long> UploadBackgroundAsync(
        AppDbContext db,
        Course course,
        IFormFile file,
        CancellationToken ct = default)
    {
        // TODO: Validation
        await using var stream = file.OpenReadStream();
        var result = await _fileService.UploadAsync(
            stream,
            file.FileName,
            file.ContentType,
            _currentUserService.UserId,
            FileCategory.CourseBackground,
            ct);
        if (!result.IsSuccess)
        {
            _logger.LogError("UploadAsync failed.");
            throw new Exception();
        }

        if (course.BackgroundFileId is not null)
            await _fileService.DeleteAsync((long)course.BackgroundFileId, ct);

        course.BackgroundFileId = result.Value;
        await db.SaveChangesAsync(ct);

        return result.Value;
    }
}
