using Backend.Api.Core.Helpers;
using Backend.Api.Core.Types;
using Backend.Api.Services.Common;
using Backend.Persistence.Data;
using Backend.Persistence.Entities.Courses;
using Microsoft.EntityFrameworkCore;

namespace Backend.Api.Services.Dashboard;

public interface IDashboardService
{
    public Task<StudentStatsResponse> GetStudentDashboardAsync(CancellationToken ct = default);
    public Task<InstructorStatsResponse> GetInstructorDashboardAsync(CancellationToken ct = default);

    public Task<List<long>> GetStudentActivityIdsAsync(
        AppDbContext db,
        ActivityType type,
        long userId,
        CancellationToken ct = default);

    public Task<List<long>> GetInstructorActivityIdsAsync(
        AppDbContext db,
        ActivityType type,
        long userId,
        CancellationToken ct = default);
}

public class DashboardService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService) : IDashboardService
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;


    public async Task<StudentStatsResponse> GetStudentDashboardAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var currentUserId = _currentUserService.UserId;

        var activeCourses = await db.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == currentUserId)
            .Select(e => e.CourseId)
            .Distinct()
            .CountAsync(ct);

        var assignmentIds = await GetStudentActivityIdsAsync(db, ActivityType.Assignment, currentUserId, ct);
        var pendingAssign = assignmentIds.Count - await db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => s.UserId == currentUserId && assignmentIds.Contains(s.AssignmentId))
            .Select(s => s.AssignmentId)
            .Distinct()
            .CountAsync(ct);

        var assessmentIds = await GetStudentActivityIdsAsync(db, ActivityType.Assessment, currentUserId, ct);
        var pendingAssess = assessmentIds.Count - await db.AssessmentAttempts
            .AsNoTracking()
            .Where(s => s.StudentId == currentUserId && assessmentIds.Contains(s.AssessmentId))
            .Select(s => s.AssessmentId)
            .Distinct()
            .CountAsync(ct);

        var problemIds = await GetStudentActivityIdsAsync(db, ActivityType.Problem, currentUserId, ct);
        var pendingProblems = problemIds.Count - await db.ProblemSubmissions
            .AsNoTracking()
            .Where(s => s.UserId == currentUserId && problemIds.Contains(s.ProblemId))
            .Select(s => s.ProblemId)
            .Distinct()
            .CountAsync(ct);

        return new StudentStatsResponse(activeCourses, pendingAssign, pendingAssess, pendingProblems);
    }

    public async Task<InstructorStatsResponse> GetInstructorDashboardAsync(CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);
        var currentUserId = _currentUserService.UserId;

        var totalCourses = db.Courses
            .AsNoTracking()
            .Where(c => c.InstructorId == currentUserId);
        var totalCourseIds = totalCourses.Select(c => c.Id);

        // NOTE: Actually total enrollment count, change the name perhaps?
        var totalStudents = await db.Enrollments
            .AsNoTracking()
            .Where(e => totalCourseIds.Contains(e.Id))
            .CountAsync(ct);

        var totalCourseCount = await totalCourses.CountAsync(ct);
        var publishedCourseCount = await totalCourses
            .Where(c => c.IsPublished)
            .CountAsync(ct);

        var assignmentIds = await GetInstructorActivityIdsAsync(db, ActivityType.Assignment, currentUserId, ct);
        var pendingSubmissions = db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => assignmentIds.Contains(s.AssignmentId) && s.Grade == null);
        var pendingAssign = await pendingSubmissions
            .Select(s => s.AssignmentId)
            .Distinct()
            .CountAsync(ct);

        var (startOfWeek, endOfWeek) = GetCurrentWeekRange.Get();
        var gradedThisWeek = await db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => assignmentIds.Contains(s.AssignmentId))
            .Where(s => s.CreatedAt >= startOfWeek &&
                        s.CreatedAt < endOfWeek)
            .CountAsync(ct);

        return new InstructorStatsResponse(totalStudents,
            publishedCourseCount,
            totalCourseCount,
            pendingAssign,
            await pendingSubmissions.CountAsync(ct),
            gradedThisWeek);
    }

    public async Task<List<long>> GetStudentActivityIdsAsync(
        AppDbContext db,
        ActivityType type,
        long userId,
        CancellationToken ct = default)
    {
        System.Linq.Expressions.Expression<Func<CourseActivity, long>> selectExpr;

        switch (type)
        {
            case ActivityType.Lesson:
                selectExpr = r => r.Lesson!.Id;
                break;
            case ActivityType.Assignment:
                selectExpr = r => r.Assignment!.Id;
                break;
            case ActivityType.Assessment:
                selectExpr = r => r.Assessment!.Id;
                break;
            case ActivityType.Problem:
                selectExpr = r => r.Problem!.Id;
                break;
            default:
                return [];
        }

        var courseIds = db.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .Select(e => e.CourseId)
            .Distinct();
        var moduleIds = await db.CourseModules
            .AsNoTracking()
            .Where(m => m.IsPublished && courseIds.Contains(m.CourseId))
            .Select(m => m.Id)
            .ToListAsync(ct);

        return await db.CourseActivities
            .AsNoTracking()
            .Where(r => r.IsPublished && r.Type == type && moduleIds.Contains(r.ModuleId))
            .Select(selectExpr)
            .ToListAsync(ct);
    }

    public async Task<List<long>> GetInstructorActivityIdsAsync(
        AppDbContext db,
        ActivityType type,
        long userId,
        CancellationToken ct = default)
    {
        System.Linq.Expressions.Expression<Func<CourseActivity, long>> selectExpr;

        switch (type)
        {
            case ActivityType.Lesson:
                selectExpr = r => r.Lesson!.Id;
                break;
            case ActivityType.Assignment:
                selectExpr = r => r.Assignment!.Id;
                break;
            case ActivityType.Assessment:
                selectExpr = r => r.Assessment!.Id;
                break;
            case ActivityType.Problem:
                selectExpr = r => r.Problem!.Id;
                break;
            default:
                return [];
        }

        var courseIds = db.Courses
            .AsNoTracking()
            .Where(c => c.InstructorId == userId)
            .Select(c => c.Id);
        var moduleIds = await db.CourseModules
            .AsNoTracking()
            .Where(m => m.IsPublished && courseIds.Contains(m.CourseId))
            .Select(m => m.Id)
            .ToListAsync(ct);

        return await db.CourseActivities
            .AsNoTracking()
            .Where(r => r.IsPublished && r.Type == type && moduleIds.Contains(r.ModuleId))
            .Select(selectExpr)
            .ToListAsync(ct);
    }
}
