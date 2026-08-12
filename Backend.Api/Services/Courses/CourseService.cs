using Backend.Api.Data;
using Backend.Api.Core.Common;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Services.Common;
using Backend.Api.Core.Types;
using Backend.Api.Core.Entities.Courses;
using AutoMapper;
using Backend.Api.Services.Content;

namespace Backend.Api.Services.Courses;

public class CourseService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    FileService fileService,
    IMapper mapper)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly FileService _fileService = fileService;
    private readonly IMapper _mapper = mapper;

    public async Task<Course?> GetAsync(long courseId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Courses
            .Where(c => c.Id == courseId)
            .FirstOrDefaultAsync();
    }

    public async Task<Course?> GetFromModuleAsync(long moduleId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.CourseModules
            .Where(m => m.Id == moduleId)
            .Select(m => m.Course)
            .FirstOrDefaultAsync();
    }

    public async Task<CourseResponse?> GetDtoAsync(long courseId)
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

    public async Task<PaginatedResponse<CourseResponse>> QueryCoursesAsync(CourseRequest query)
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

        return new PaginatedResponse<CourseResponse>(
                query.PageNumber,
                query.PageSize,
                await courses.CountAsync(),
                list);
    }

    public async Task<long> UploadBackgroundAsync(
        Course course,
        IFormFile file,
        CancellationToken ct)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);

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

    public async Task<CourseSetResponse> CreateCourseAsync(CourseSetRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var course = new Course
        {
            CreatorId = _currentUserService.UserId,
            Title = request.Title,
            Description = request.Description,
            IsPublished = request.IsPublished,
            EnrollmentOpen = request.EnrollmentOpen
        };
        db.Courses.Add(course);
        await db.SaveChangesAsync();
        return _mapper.Map<CourseSetResponse>(course);
    }

    public async Task<CourseSetResponse> UpdateCourseAsync(Course course, CourseSetRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        course.Title = request.Title;
        course.Description = request.Description;
        course.IsPublished = request.IsPublished;
        course.EnrollmentOpen = request.EnrollmentOpen;

        db.Courses.Update(course);
        await db.SaveChangesAsync();
        return _mapper.Map<CourseSetResponse>(course);
    }

    public async Task DeleteCourseAsync(Course course)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        db.Courses.Remove(course);
        await db.SaveChangesAsync();
        return;
    }

    public async Task SetCoursePublishStatusAsync(Course course, bool value)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        course.IsPublished = value;
        await db.SaveChangesAsync();
        return;
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
}
