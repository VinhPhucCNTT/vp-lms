using AutoMapper;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Api.Services.Common;
using Backend.Api.Services.Content;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Courses;
using Microsoft.EntityFrameworkCore;
using Sqids;

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
    SqidsEncoder<long> sqidsEncoder,
    ILogger<CourseService> logger) : ICourseService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IFileService _fileService = fileService;
    private readonly IMapper _mapper = mapper;
    private readonly SqidsEncoder<long> _sqidsEncoder = sqidsEncoder;
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

        var list = await ProjectCourseDetails(courses
            .OrderBy(c => c.Title)
            .Skip((page.PageNumber - 1) * page.PageSize)
            .Take(page.PageSize))
        .ToListAsync(ct);

        List<StudentCourseResponse> results = [];
        foreach (var c in list)
        {
            var item = new StudentCourseResponse(
                ToCourseDetailResponse(c),
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
        var publishedCourses = db.Courses
            .AsNoTracking()
            .Where(c => c.IsPublished);

        var featuredCourses = (await ProjectCourseDetails(publishedCourses
                .OrderByDescending(c => c.Title)
                .Take(10))
            .ToListAsync(ct))
        .ConvertAll(ToCourseDetailResponse);

        var departmentCourses = await publishedCourses
            .Where(c => c.Department != null)
            .Select(c => new DepartmentCourseRow(
                c.Department!.Name,
                new CourseDetailRow(
                    c.Id,
                    c.InstructorId,
                    c.Instructor.FirstName,
                    c.Instructor.LastName,
                    c.BackgroundFileId,
                    c.Title,
                    c.Description,
                    c.Enrollments.Count,
                    c.EnrollmentOpen)))
            .ToListAsync(ct);

        var coursesByDepartment = departmentCourses
            .GroupBy(c => c.DepartmentName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(c => ToCourseDetailResponse(c.Course)).ToList());

        var recentlyUpdated = (await ProjectCourseDetails(publishedCourses
                .OrderByDescending(c => c.UpdatedAt)
                .Take(10))
            .ToListAsync(ct))
        .ConvertAll(ToCourseDetailResponse);

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

        if (request.BackgroundFile is not null)
            await UploadBackgroundAsync(db, course, request.BackgroundFile, ct);

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

    private static IQueryable<CourseDetailRow> ProjectCourseDetails(IQueryable<Course> courses)
        => courses.Select(c => new CourseDetailRow(
            c.Id,
            c.InstructorId,
            c.Instructor.FirstName,
            c.Instructor.LastName,
            c.BackgroundFileId,
            c.Title,
            c.Description,
            c.Enrollments.Count,
            c.EnrollmentOpen));

    private CourseDetailResponse ToCourseDetailResponse(CourseDetailRow course)
        => new(
            _sqidsEncoder.Encode(course.Id),
            _sqidsEncoder.Encode(course.InstructorId),
            course.InstructorFirstName,
            course.InstructorLastName,
            course.BackgroundFileId is long backgroundFileId
                ? _sqidsEncoder.Encode(backgroundFileId)
                : null,
            course.Title,
            course.Description,
            course.EnrollmentCount,
            course.EnrollmentOpen);

    private sealed record CourseDetailRow(
        long Id,
        long InstructorId,
        string InstructorFirstName,
        string InstructorLastName,
        long? BackgroundFileId,
        string Title,
        string? Description,
        int EnrollmentCount,
        bool EnrollmentOpen);

    private sealed record DepartmentCourseRow(
        string DepartmentName,
        CourseDetailRow Course);

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
