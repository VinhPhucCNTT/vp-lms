using Backend.Api.Data;
using Microsoft.EntityFrameworkCore;
using Backend.Api.Services.Common;
using Sqids;
using AutoMapper;
using Backend.Api.Core.Types;
using Backend.Api.Core.Entities.Assessments;
using Backend.Api.Core.Common;
using System.Text.Json;

namespace Backend.Api.Services.Assessments;

public sealed class QuestionService(
    IDbContextFactory<AppDbContext> dbFactory,
    CurrentUserService currentUserService,
    IMapper mapper,
    SqidsEncoder<long> sqidsEncoder)
{
    private readonly IDbContextFactory<AppDbContext> _dbFactory = dbFactory;
    private readonly CurrentUserService _currentUserService = currentUserService;
    private readonly IMapper _mapper = mapper;
    private readonly SqidsEncoder<long> _sqidsEncoder = sqidsEncoder;

    // public async Task<Question?> GetByIdAsync(
    //     long questionId,
    //     CancellationToken ct = default)
    // {
    //     using var db = await _dbFactory.CreateDbContextAsync(ct);
    //     return await db.Questions
    //         .AsNoTracking()
    //         .FirstOrDefaultAsync(x => x.Id == questionId, ct);
    // }

    public async Task<Result<QuestionResponse>> CreateAsync(
           long bankId,
           QuestionRequest request,
           CancellationToken ct = default)
    {
        var instructorId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var bank = await db.QuestionBanks
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == bankId, ct);

        if (bank is null)
            return Result<QuestionResponse>.Failure(
                QuestionErrors.NotFound);

        if (bank.OwnerId != instructorId)
            return Result<QuestionResponse>.Failure(
                QuestionErrors.Forbidden);

        var validation = contentValidator.Validate(
            request.QuestionType,
            request.QuestionData);

        if (!validation.IsSuccess)
        {
            return Result<Question>.Failure(
                validation.Errors.ToArray());
        }

        var question = new Question
        {
            QuestionBankId = bankId,
            QuestionType = request.QuestionType,
            QuestionData = request.QuestionData
        };

        db.Questions.Add(question);
        await db.SaveChangesAsync(ct);
        return Result<QuestionResponse>.Success(_mapper.Map<QuestionResponse>(question));
    }

    public async Task<Result> UpdateAsync(
        long questionId,
        QuestionRequest request,
        CancellationToken ct = default)
    {
        var instructorId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var question = await db.Questions
            .Include(x => x.QuestionBank)
            .FirstOrDefaultAsync(x => x.Id == questionId, ct);

        if (question is null)
            return Result.Failure(QuestionErrors.NotFound);

        if (question.QuestionBank.OwnerId != instructorId)
            return Result.Failure(QuestionErrors.Forbidden);

        var usedByAttempt = await db.AttemptQuestions
            .AnyAsync(x => x.AssessmentQuestionId == questionId, ct);

        if (usedByAttempt)
            return Result.Failure(QuestionErrors.HasAttempts);

        var validation = contentValidator.Validate(
            request.QuestionType,
            request.QuestionData);

        if (!validation.IsSuccess)
            return Result.Failure(validation.Errors.ToArray());

        question.QuestionType = request.QuestionType;
        question.QuestionData = request.QuestionData;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(
           long questionId,
           CancellationToken ct = default)
    {
        var instructorId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var question = await db.Questions
            .Include(x => x.QuestionBank)
            .FirstOrDefaultAsync(x => x.Id == questionId, ct);

        if (question is null)
            return Result.Failure(QuestionErrors.NotFound);

        if (question.QuestionBank.OwnerId != instructorId)
            return Result.Failure(QuestionErrors.Forbidden);

        question.IsDeleted = true;

        await db.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<QuestionResponse>> CopyAsync(
           long sourceQuestionId,
           long destinationBankId,
           CancellationToken ct = default)
    {
        var instructorId = _currentUserService.UserId;
        await using var db = await _dbFactory.CreateDbContextAsync(ct);

        var destination = await db.QuestionBanks
            .FirstOrDefaultAsync(x => x.Id == destinationBankId, ct);

        if (destination is null)
            return Result<QuestionResponse>.Failure(
                QuestionErrors.NotFound);

        if (destination.OwnerId != instructorId)
            return Result<QuestionResponse>.Failure(
                QuestionErrors.Forbidden);

        var source = await db.Questions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == sourceQuestionId, ct);

        if (source is null)
            return Result<QuestionResponse>.Failure(
                QuestionErrors.NotFound);

        var copy = new Question
        {
            QuestionBankId = destinationBankId,
            QuestionType = source.QuestionType,
            QuestionData = JsonDocument.Parse(
                source.QuestionData.RootElement.GetRawText())
        };

        db.Questions.Add(copy);
        await db.SaveChangesAsync(ct);

        return Result<QuestionResponse>.Success(_mapper.Map<QuestionResponse>(copy));
    }

    // private async Task EnsureQuestionCanBeModifiedAsync(
    //     long questionId,
    //     CancellationToken ct)
    // {
    //     using var db = await _dbFactory.CreateDbContextAsync(ct);
    //     var used = await db.AttemptQuestions
    //         .AnyAsync(
    //             x => x.QuestionId == questionId,
    //             ct);
    //
    //     if (used)
    //     {
    //         throw new ValidationException(
    //             "This question has already been used in an assessment attempt.");
    //     }
    // }
    //
    // private static void ValidateQuestionContent(
    //     QuestionType type,
    //     JsonDocument content)
    // {
    //     // deserialize and validate based on type
    // }
}
