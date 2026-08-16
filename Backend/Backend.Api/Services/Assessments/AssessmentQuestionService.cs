using Backend.Persistence.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Services.Common;
using AutoMapper;
using Backend.Api.Core.Types;
using Backend.Api.Core.Common;
using Backend.Persistence.Entities.Assessments;
using System.Text.Json;
using Backend.Api.Services.Assessments.Validators;

namespace Backend.Api.Services.Assessments;

public class AssessmentQuestionService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper,
    IQuestionContentValidator contentValidator)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;
    private readonly IQuestionContentValidator _contentValidator = contentValidator;

    public async Task<Result<IReadOnlyList<AssessmentQuestionInfo>>>
        GetQuestionsAsync(
            long assessmentId,
            CancellationToken ct = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var questions = await db.AssessmentQuestions
            .AsNoTracking()
            .Where(x => x.AssessmentId == assessmentId)
            .OrderBy(x => x.OrderIndex)
            .Select(x => _mapper.Map<AssessmentQuestionInfo>(x))
            .ToListAsync(ct);

        return Result<IReadOnlyList<AssessmentQuestionInfo>>
            .Success(questions);
    }

    public async Task<Result<AssessmentQuestionInfo>> AddAsync(
        long resourceId,
        AssessmentQuestionInfo request,
        CancellationToken ct = default)
    {
        using var db = await _dbFactory.CreateDbContextAsync(ct);
        var userId = _currentUserService.UserId;

        var assessment = await db.Assessments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == resourceId, ct);
        if (assessment is null)
            return Result<AssessmentQuestionInfo>.Failure(AssessmentErrors.NotFound);

        // await EnsureCanModifyQuestionsAsync(
        //     assessment,
        //     instructorId,
        //     ct);

        var mutable = await CanModifyQuestionsAsync(db, assessment.Id, ct);

        if (!mutable)
            return Result<AssessmentQuestionInfo>.Failure(AssessmentErrors.HasAttempts);

        var validation = _contentValidator.Validate(
            request.QuestionType,
            request.QuestionData);

        if (!validation.IsSuccess)
        {
            return Result<AssessmentQuestionInfo>.Failure(
                [.. validation.Errors]);
        }

        var nextIndex = await db.AssessmentQuestions
            .Where(x => x.AssessmentId == assessment.Id)
            .Select(x => (int?)x.OrderIndex)
            .MaxAsync(ct) ?? -1;

        var question = new AssessmentQuestion
        {
            AssessmentId = assessment.Id,
            Text = request.Text,
            QuestionType = request.QuestionType,
            QuestionData = request.QuestionData,
            OrderIndex = nextIndex + 1,
            Points = request.Points
        };

        db.AssessmentQuestions.Add(question);
        await db.SaveChangesAsync(ct);

        return Result<AssessmentQuestionInfo>.Success(_mapper.Map<AssessmentQuestionInfo>(question));
    }

    public async Task<Result<AssessmentQuestionInfo>> AddFromBankAsync(
           long resourceId,
           long questionId,
           decimal points,
           CancellationToken ct = default)
    {
        var instructorId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var assessment = await db.Assessments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == resourceId, ct);

        if (assessment is null)
            return Result<AssessmentQuestionInfo>.Failure(
                AssessmentErrors.NotFound);

        if (!await CanModifyQuestionsAsync(db, resourceId, ct))
        {
            return Result<AssessmentQuestionInfo>.Failure(
                AssessmentErrors.HasAttempts);
        }

        var source = await db.Questions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == questionId, ct);

        if (source is null)
            return Result<AssessmentQuestionInfo>.Failure(
                QuestionErrors.NotFound);

        var questionData = CloneJson(source.QuestionData);

        var validation = _contentValidator.Validate(
            source.QuestionType,
            questionData);

        if (!validation.IsSuccess)
        {
            return Result<AssessmentQuestionInfo>.Failure(
                [.. validation.Errors]);
        }

        var nextIndex = await db.AssessmentQuestions
            .Where(x => x.AssessmentId == resourceId)
            .Select(x => (int?)x.OrderIndex)
            .MaxAsync(ct) ?? -1;

        var assessmentQuestion = new AssessmentQuestion
        {
            AssessmentId = resourceId,
            QuestionId = source.Id,

            // Snapshot of the bank question.
            QuestionType = source.QuestionType,
            QuestionData = questionData,

            OrderIndex = nextIndex + 1,
            Points = points
        };

        db.AssessmentQuestions.Add(assessmentQuestion);
        await db.SaveChangesAsync(ct);

        return Result<AssessmentQuestionInfo>.Success(
            _mapper.Map<AssessmentQuestionInfo>(assessmentQuestion));
    }

    public async Task<Result> UpdateAsync(
           long assessmentId,
           long questionId,
           AssessmentQuestionInfo request,
           CancellationToken ct = default)
    {
        var instructorId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var assessment = await db.Assessments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == assessmentId, ct);

        if (assessment is null)
            return Result.Failure(AssessmentErrors.NotFound);

        if (!await CanModifyQuestionsAsync(db, assessmentId, ct))
            return Result.Failure(AssessmentErrors.HasAttempts);

        var question = await db.AssessmentQuestions
            .FirstOrDefaultAsync(
                x => x.AssessmentId == assessmentId &&
                     x.Id == questionId,
                ct);

        if (question is null)
            return Result.Failure(
                new Error(
                    "assessment_question.not_found",
                    "Assessment question was not found."));

        var validation = _contentValidator.Validate(
            request.QuestionType,
            request.QuestionData);

        if (!validation.IsSuccess)
            return Result.Failure([.. validation.Errors]);

        question.QuestionType = request.QuestionType;
        question.QuestionData = request.QuestionData;
        question.Points = request.Points;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> RemoveAsync(
            long assessmentId,
            long questionId,
            CancellationToken ct = default)
    {
        var instructorId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var assessment = await db.Assessments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == assessmentId, ct);

        if (assessment is null)
            return Result.Failure(AssessmentErrors.NotFound);

        if (!await CanModifyQuestionsAsync(db, assessmentId, ct))
            return Result.Failure(AssessmentErrors.HasAttempts);

        var question = await db.AssessmentQuestions
            .FirstOrDefaultAsync(
                x => x.AssessmentId == assessmentId &&
                     x.Id == questionId,
                ct);

        if (question is null)
        {
            return Result.Failure(
                new Error(
                    "assessment_question.not_found",
                    "Assessment question was not found."));
        }

        db.AssessmentQuestions.Remove(question);

        await db.SaveChangesAsync(ct);

        // Normalize separately so the deleted row isn't involved.
        var remaining = await db.AssessmentQuestions
            .Where(x => x.AssessmentId == assessmentId)
            .OrderBy(x => x.OrderIndex)
            .ToListAsync(ct);

        for (var i = 0; i < remaining.Count; i++)
            remaining[i].OrderIndex = i;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> ReorderAsync(
           long assessmentId,
           IReadOnlyList<long> questionIds,
           CancellationToken ct = default)
    {
        var instructorId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var assessment = await db.Assessments
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == assessmentId, ct);

        if (assessment is null)
            return Result.Failure(AssessmentErrors.NotFound);

        if (!await CanModifyQuestionsAsync(db, assessmentId, ct))
            return Result.Failure(AssessmentErrors.HasAttempts);

        var questions = await db.AssessmentQuestions
            .Where(x => x.AssessmentId == assessmentId)
            .ToListAsync(ct);

        if (questions.Count != questionIds.Count)
        {
            return Result.Failure(
                new Error(
                    "assessment_question.invalid_order",
                    "The supplied order does not contain all assessment questions."));
        }

        var lookup = questions.ToDictionary(x => x.Id);

        if (questionIds.Distinct().Count() != questionIds.Count ||
            questionIds.Any(x => !lookup.ContainsKey(x)))
        {
            return Result.Failure(
                new Error(
                    "assessment_question.invalid_order",
                    "The supplied question order is invalid."));
        }

        for (var i = 0; i < questionIds.Count; i++)
            lookup[questionIds[i]].OrderIndex = i;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    private static async Task<bool> CanModifyQuestionsAsync(
           AppDbContext db,
           long assessmentId,
           CancellationToken ct)
    {
        var hasAttempts = await db.AssessmentAttempts
            .AnyAsync(x => x.AssessmentId == assessmentId, ct);

        return !hasAttempts;
    }

    // private async Task NormalizeOrderAsync(
    //     long assessmentId,
    //     CancellationToken ct)
    // {
    //     using var db = await _dbFactory.CreateDbContextAsync(ct);
    //     var questions = await db.AssessmentQuestions
    //         .Where(x => x.AssessmentId == assessmentId)
    //         .OrderBy(x => x.OrderIndex)
    //         .ToListAsync(ct);
    //
    //     for (var i = 0; i < questions.Count; i++)
    //     {
    //         questions[i].OrderIndex = i;
    //     }
    //
    //     await db.SaveChangesAsync(ct);
    // }

    private static JsonDocument CloneJson(JsonDocument source) =>
        JsonDocument.Parse(source.RootElement.GetRawText());
}
