using Backend.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Services.Common;
using Backend.Core.Types;
using Backend.Core.Entities.Courses;
using Sqids;
using Backend.Core.Entities.Resources;
using Backend.Core.Entities.Submissions;

namespace Backend.Services.Content;

public class AssessmentService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    SqidsEncoder<long> sqidsEncoder)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly SqidsEncoder<long> _sqidsEncoder = sqidsEncoder;

    public async Task<AssessmentResponseDto?> GetAssessmentByIdAsync(long resourceId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        return await db.Assessments
            .AsNoTracking()
            .Where(a => a.ResourceId == resourceId)
            .Select(a => new AssessmentResponseDto(
                _sqidsEncoder.Encode(a.Id),
                a.InstructionsMarkdown,
                a.TimeLimitMinutes,
                a.MaxAttempts,
                a.ShuffleQuestions,
                a.ShowResults,
                a.GradingSchemaJson
            )).FirstOrDefaultAsync();
    }

    public async Task<AssessmentResponseDto> CreateAssessmentAsync(ModuleResource resource, AssessmentRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var assessment = new Assessment
        {
            ResourceId = resource.Id,
            InstructionsMarkdown = request.InstructionsMarkdown,
            TimeLimitMinutes = request.TimeLimitMinutes,
            MaxAttempts = request.MaxAttempts,
            ShuffleQuestions = request.ShuffleQuestions,
            ShowResults = request.ShowResults,
            GradingSchemaJson = request.GradingSchemaJson
        };
        db.Assessments.Add(assessment);
        await db.SaveChangesAsync();
        return new AssessmentResponseDto(
            _sqidsEncoder.Encode(assessment.Id),
            assessment.InstructionsMarkdown,
            assessment.TimeLimitMinutes,
            assessment.MaxAttempts,
            assessment.ShuffleQuestions,
            assessment.ShowResults,
            assessment.GradingSchemaJson
        );
    }

    public async Task<AssessmentResponseDto?> UpdateAssessmentAsync(long assessmentId, AssessmentRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var assessment = await db.Assessments.FirstOrDefaultAsync(a => a.Id == assessmentId);
        if (assessment is null)
            return null;

        assessment.InstructionsMarkdown = request.InstructionsMarkdown;
        assessment.TimeLimitMinutes = request.TimeLimitMinutes;
        assessment.MaxAttempts = request.MaxAttempts;
        assessment.ShuffleQuestions = request.ShuffleQuestions;
        assessment.ShowResults = request.ShowResults;
        assessment.GradingSchemaJson = request.GradingSchemaJson;

        db.Assessments.Update(assessment);
        await db.SaveChangesAsync();
        return new AssessmentResponseDto(
            _sqidsEncoder.Encode(assessment.Id),
            assessment.InstructionsMarkdown,
            assessment.TimeLimitMinutes,
            assessment.MaxAttempts,
            assessment.ShuffleQuestions,
            assessment.ShowResults,
            assessment.GradingSchemaJson
        );
    }

    public async Task<List<QuestionResponse>?> SetQuestionsAsync(long assessmentId, List<QuestionRequest> requests)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var assessment = await db.Assessments.FirstOrDefaultAsync(a => a.Id == assessmentId);
        if (assessment is null)
            return null;

        List<QuestionResponse> questions = [];
        foreach (var request in requests)
        {
            long questionId = _sqidsEncoder.Decode(request.QuestionSqid).SingleOrDefault();
            AssessmentQuestion? question;

            if (questionId == default)
            {
                question = new AssessmentQuestion
                {
                    AssessmentId = assessment.Id,
                    QuestionType = request.QuestionType,
                    QuestionTextMarkdown = request.QuestionTextMarkdown,
                    Points = request.Points,
                    OrderIndex = request.OrderIndex,
                    QuestionDataJson = request.QuestionDataJson
                };
                db.AssessmentQuestions.Add(question);
            }
            else
            {
                question = await db.AssessmentQuestions.FirstOrDefaultAsync(q => q.Id == questionId);
                if (question is null)
                    continue;

                question.AssessmentId = assessment.Id;
                question.QuestionType = request.QuestionType;
                question.QuestionTextMarkdown = request.QuestionTextMarkdown;
                question.Points = request.Points;
                question.OrderIndex = request.OrderIndex;
                question.QuestionDataJson = request.QuestionDataJson;

                db.AssessmentQuestions.Update(question);
            }

            questions.Add(new QuestionResponse(
                _sqidsEncoder.Encode(question.Id),
                question.QuestionType,
                question.QuestionTextMarkdown,
                question.Points,
                question.OrderIndex,
                question.QuestionDataJson
            ));
        }

        await db.SaveChangesAsync();
        return questions;
    }

    public async Task<AssessmentAttemptResponse?> GetLatestAttemptAsync(long assessmentId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.AssessmentAttempts
            .AsNoTracking()
            .Where(a => a.AssessmentId == assessmentId && a.UserId == currentUserId)
            .OrderByDescending(a => a.AttemptNumber)
            .Select(a => new AssessmentAttemptResponse(
                _sqidsEncoder.Encode(a.Id),
                a.StartedAt,
                a.SubmittedAt,
                a.TotalScore,
                a.IsPassed,
                a.AttemptNumber
            )).FirstOrDefaultAsync();
    }

    public async Task<List<AssessmentAttemptResponse>> GetAttemptsAsync(long assessmentId)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var currentUserId = _currentUserService.UserId;
        return await db.AssessmentAttempts
            .AsNoTracking()
            .Where(a => a.AssessmentId == assessmentId && a.UserId == currentUserId)
            .OrderByDescending(a => a.AttemptNumber)
            .Select(a => new AssessmentAttemptResponse(
                _sqidsEncoder.Encode(a.Id),
                a.StartedAt,
                a.SubmittedAt,
                a.TotalScore,
                a.IsPassed,
                a.AttemptNumber
            )).ToListAsync();
    }

    // PLACEHOLDER
    public async Task<AssessmentAttemptResponse?> StartAttemptAsync(long assessmentId, AssessmentAttemptRequest request)
    {
        using var db = await _dbFactory.CreateDbContextAsync();
        var assessment = await db.Assessments.FirstOrDefaultAsync(a => a.Id == assessmentId);
        if (assessment is null)
            return null;

        // TODO: Figure out what to do if there is multiple unfinished attempts (bug)
        // Frontend warns?
        var currentUserId = _currentUserService.UserId;
        var previousAttempts = db.AssessmentAttempts
            .Where(a => a.AssessmentId == assessment.Id && a.UserId == currentUserId)
            .OrderByDescending(a => a.AttemptNumber);
        var previousAttempt = await previousAttempts.FirstOrDefaultAsync();
        if (previousAttempt is not null && previousAttempt.IsPassed is null)
            return null; // There is an ongoing attempt, frontend should prevent this

        var attempt = new AssessmentAttempt
        {
            AssessmentId = assessmentId,
            UserId = currentUserId,
            AttemptNumber = await previousAttempts.CountAsync() + 1
        };
        db.AssessmentAttempts.Add(attempt);
        await db.SaveChangesAsync();
        return new AssessmentAttemptResponse(
            _sqidsEncoder.Encode(attempt.Id),
            attempt.StartedAt,
            null,
            null,
            null,
            attempt.AttemptNumber
        );
    }

    // public async Task<AssessmentAttemptResponse?> SubmitAttemptAsync(long assessmentId, AssessmentAttemptResponse request) { }

    // public async Task GradeAttemptAsync() { }

    // public async Task GetAssessmentStatsAsync() { }
}
