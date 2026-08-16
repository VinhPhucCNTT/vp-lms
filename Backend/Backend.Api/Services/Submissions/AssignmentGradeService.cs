using Backend.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Services.Common;
using Backend.Api.Core.Types;
using AutoMapper;
using Backend.Persistence.Entities.Assignments;
using Backend.Api.Core.Common;

namespace Backend.Api.Services.Submissions;

public class AssignmentGradeService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;

    public async Task<AssignmentGradeResponse?> GetGradeAsync(long submissionId, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await db.AssignmentGrades
            .AsNoTracking()
            .Where(g => g.SubmissionId == submissionId)
            .Select(g => _mapper.Map<AssignmentGradeResponse>(g))
            .FirstOrDefaultAsync(ct);
    }

    public async Task<PaginatedResponse<AssignmentGradeResponse>?> GetAssignmentGradesAsync(long resourceId, PageRequest page, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await GetGradesAsync(db, page, g => g.Submission.AssignmentId == resourceId, ct);
    }

    public async Task<PaginatedResponse<AssignmentGradeResponse>?> GetInstructorGradedAsync(PageRequest page, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var currentUserId = _currentUserService.UserId;
        return await GetGradesAsync(db, page, g => g.GraderId == currentUserId, ct);
    }

    public async Task<PaginatedResponse<AssignmentGradeResponse>?> GetStudentSelfGradesAsync(PageRequest page, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var currentUserId = _currentUserService.UserId;
        return await GetGradesAsync(db, page, g => g.Submission.UserId == currentUserId, ct);
    }

    public async Task<PaginatedResponse<AssignmentGradeResponse>?> GetStudentGradesAsync(long resourceId, long studentUserId, PageRequest page, CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        return await GetGradesAsync(db, page, g => g.Submission.AssignmentId == resourceId && g.Submission.UserId == studentUserId, ct);
    }

    public async Task<AssignmentGradeResponse?> GradeAsync(long submissionId, AssignmentGradeRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var submission = await db.AssignmentSubmissions.FirstOrDefaultAsync(s => s.Id == submissionId && s.SubmittedOn != null);
        if (submission is null)
            return null;

        var userId = _currentUserService.UserId;
        var grade = new AssignmentGrade
        {
            SubmissionId = submission.Id,
            GraderId = userId,
            Score = request.Score,
            FeedbackText = request.FeedbackText
        };

        db.AssignmentGrades.Add(grade);
        await db.SaveChangesAsync();
        return _mapper.Map<AssignmentGradeResponse>(grade);
    }

    private async Task<PaginatedResponse<AssignmentGradeResponse>?> GetGradesAsync(
        AppDbContext db,
        PageRequest page,
        System.Linq.Expressions.Expression<Func<AssignmentGrade, bool>> predicate,
        CancellationToken ct = default)
    {
        var grades = db.AssignmentGrades
            .AsNoTracking()
            .Where(predicate)
            .Select(g => _mapper.Map<AssignmentGradeResponse>(g));

        var list = await grades
        .Skip((page.PageNumber - 1) * page.PageSize)
        .Take(page.PageSize)
        .ToListAsync(ct);

        return new PaginatedResponse<AssignmentGradeResponse>(
            page.PageNumber,
            page.PageSize,
            await grades.CountAsync(ct),
            list);
    }
}
