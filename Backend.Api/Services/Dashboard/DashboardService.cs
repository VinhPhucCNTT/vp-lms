using Backend.Api.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Core.Types;
using Backend.Api.Core.Entities.Courses;

namespace Backend.Api.Services.Dashboard;

public class DashboardService(
    IDbContextFactory<AppDbContext> dbFactory)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;

    public async Task<StudentDashboardResponse?> GetStudentDashboardAsync(long studentUserId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();

        var activeCourses = db.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == studentUserId)
            .DistinctBy(e => e.CourseId)
            .CountAsync();
        var pendingAssign = CountPendingAssignmentsAsync(db, studentUserId);
        var pendingAssess = CountPendingAssessmentsAsync(db, studentUserId);
        var pendingProblems = CountPendingProblemsAsync(db, studentUserId);

        var results = await Task.WhenAll(activeCourses, pendingAssign, pendingAssess, pendingProblems);
        var stats = new StudentDashboardStats(results[0], results[1], results[2], results[3]);
    }

    public async Task<InstructorDashboardResponse?> GetInstructorDashboardAsync(long instructorUserId)
    {
    }

    static private async Task<int> CountPendingAssignmentsAsync(AppDbContext db, long userId)
    {
        var assignmentIds = await GetCourseResourceIdsAsync(db, ResourceType.Assignment, userId);
        return assignmentIds.Count - await db.AssignmentSubmissions
            .AsNoTracking()
            .Where(s => s.UserId == userId && assignmentIds.Contains(s.AssignmentId))
            .DistinctBy(s => s.AssignmentId)
            .CountAsync();
    }

    static private async Task<int> CountPendingAssessmentsAsync(AppDbContext db, long userId)
    {
        var assessmentIds = await GetCourseResourceIdsAsync(db, ResourceType.Assessment, userId);
        return assessmentIds.Count - await db.AssessmentAttempts
            .AsNoTracking()
            .Where(s => s.UserId == userId && assessmentIds.Contains(s.AssessmentId))
            .DistinctBy(s => s.AssessmentId)
            .CountAsync();
    }

    static private async Task<int> CountPendingProblemsAsync(AppDbContext db, long userId)
    {
        var problemIds = await GetCourseResourceIdsAsync(db, ResourceType.Problem, userId);
        return problemIds.Count - await db.ProblemSubmissions
            .AsNoTracking()
            .Where(s => s.UserId == userId && problemIds.Contains(s.ProblemId))
            .DistinctBy(s => s.ProblemId)
            .CountAsync();
    }

    static private async Task<List<long>> GetCourseResourceIdsAsync(
        AppDbContext db,
        ResourceType type,
        long userId)
    {
        System.Linq.Expressions.Expression<Func<CourseResource, long>> selectExpr;

        switch (type)
        {
            case ResourceType.Lesson:
                selectExpr = r => r.Lesson!.Id;
                break;
            case ResourceType.Assignment:
                selectExpr = r => r.Assignment!.Id;
                break;
            case ResourceType.Assessment:
                selectExpr = r => r.Assessment!.Id;
                break;
            case ResourceType.Problem:
                selectExpr = r => r.Problem!.Id;
                break;
            default:
                return [];
        }

        var courseIds = db.Enrollments
            .AsNoTracking()
            .Where(e => e.UserId == userId)
            .DistinctBy(e => e.CourseId)
            .Select(e => e.CourseId);
        var moduleIds = await db.CourseModules
            .AsNoTracking()
            .Where(m => m.IsPublished && courseIds.Contains(m.CourseId))
            .Select(m => m.Id)
            .ToListAsync();

        return await db.CourseResources
            .AsNoTracking()
            .Where(r => r.IsPublished && r.Type == type && moduleIds.Contains(r.ModuleId))
            .Select(selectExpr)
            .ToListAsync();
    }

}
