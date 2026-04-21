using Backend.Data.Repositories;
using Backend.Data.UnitOfWork;
using Backend.Models.Assessments;
using Backend.Features.Assessments.Dtos.Modify;
using Backend.Features.Assessments.Dtos.Submission;
using Backend.Features.Assessments.Dtos.View;
using Microsoft.AspNetCore.Identity;
using Backend.Models.Users;
using Backend.Models.Enums;

namespace Backend.Features.Assessments.Services;

public class AssessmentService(
    IAssessmentRepository assessmentRepo,
    IQuestionRepository questionRepo,
    UserManager<ApplicationUser> userManager,
    IUnitOfWork uow) : IAssessmentService
{
    private readonly IAssessmentRepository _assessmentRepo = assessmentRepo;
    private readonly IQuestionRepository _questionRepo = questionRepo;
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IUnitOfWork _uow = uow;

    public async Task<Guid> CreateAsync(
        Guid activityId,
        CreateAssessmentDto dto)
    {
        var assessment = new Assessment
        {
            ActivityId = activityId,
            Type = dto.Type,
            TimeLimitMinutes = dto.TimeLimitMinutes,
            MaxAttempts = dto.MaxAttempts,
            Password = dto.Password,
            PassingScore = dto.PassingScore,
            ShuffleQuestions = dto.ShuffleQuestions
        };

        _assessmentRepo.Add(assessment);

        await _uow.SaveChangesAsync();

        return assessment.Id;
    }

    public async Task<ViewAssessmentDto?> GetByIdAsync(Guid id)
    {
        var assessment = await _assessmentRepo.GetByIdAsync(id);
        if (assessment is null)
            return null;

        return new ViewAssessmentDto(
            assessment.Id,
            assessment.Type,
            assessment.TimeLimitMinutes,
            assessment.MaxAttempts,
            assessment.PassingScore,
            assessment.ShuffleQuestions
        );
    }

    public async Task<List<ViewQuestionDto>?> GetAssessmentQuestionsAsync(Guid id)
    {
        var assessment = await _assessmentRepo.GetFullContentByIdAsync(id);
        if (assessment is null)
            return null;

        return assessment.Questions
            .OrderBy(q => q.OrderIndex)
            .Select(q => new ViewQuestionDto(
                q.Id,
                q.QuestionText,
                q.Type,
                q.Points,
                q.OrderIndex,
                q.Options
                    .Select(o => new ViewQuestionOptionDto(
                        o.Id,
                        o.OptionText,
                        o.IsCorrect
                    )).ToList()
            )).ToList();
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateAssessmentDto dto)
    {
        var assessment = await _assessmentRepo.GetByIdAsync(id);
        if (assessment is null)
            return false;

        assessment.Type = dto.Type;
        assessment.TimeLimitMinutes = dto.TimeLimitMinutes;
        assessment.MaxAttempts = dto.MaxAttempts;
        assessment.Password = dto.Password;
        assessment.PassingScore = dto.PassingScore;
        assessment.ShuffleQuestions = dto.ShuffleQuestions;

        _assessmentRepo.Update(assessment);

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var assessment = await _assessmentRepo.GetByIdAsync(id);
        if (assessment is null)
            return false;

        _assessmentRepo.Remove(assessment);

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RestoreAsync(Guid id)
    {
        var assessment = await _assessmentRepo.GetDeletedByIdAsync(id);
        if (assessment is null)
            return false;

        _assessmentRepo.Restore(assessment);

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> HardDeleteAsync(Guid id)
    {
        var assessment = await _assessmentRepo.GetDeletedByIdAsync(id);
        if (assessment is null)
            return false;

        _assessmentRepo.HardDelete(assessment);

        await _uow.SaveHardChangesAsync();
        return true;
    }

    public async Task<AssessmentResultDto?> SubmitAttemptAsync(
        Guid assessmentId,
        Guid studentId,
        SubmitAssessmentDto dto)
    {
        var assessment = await _assessmentRepo.GetFullContentByIdAsync(assessmentId);
        if (assessment is null)
            return null;

        var student = await _userManager.FindByIdAsync(studentId.ToString());
        if (student is null)
            return null;

        decimal score = 0;
        foreach (var answer in dto.Answers)
        {
            var question = await _questionRepo.GetQuestionByIdAsync(answer.QuestionId);
            if (question is null)
                continue;

            if (IsQuestionCorrect(answer, question))
                score += question.Points;
        }

        var passed = false;
        if (score >= assessment.PassingScore)
            passed = true;

        return new AssessmentResultDto(score, passed);
    }

    public async Task<Guid?> AddQuestionAsync(
        Guid assessmentId,
        CreateAssessmentQuestionBody dto)
    {
        var assessment = await _assessmentRepo.GetByIdAsync(assessmentId);
        if (assessment is null)
            return null;

        var question = new AssessmentQuestion
        {
            Id = Guid.NewGuid(),
            AssessmentId = assessmentId,
            QuestionText = dto.QuestionText,
            Type = dto.Type,
            Points = dto.Points,
            OrderIndex = dto.OrderIndex
        };

        foreach (var o in dto.Options)
        {
            question.Options.Add(new QuestionOption
            {
                Id = Guid.NewGuid(),
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect
            });
        }

        _questionRepo.Add(question);

        await _uow.SaveChangesAsync();

        return question.Id;
    }

    private static bool IsQuestionCorrect(SubmitAnswerDto answer, AssessmentQuestion question)
    {
        bool result = false;

        if (question.Type == QuestionType.SingleChoice)
        {
            var answerText = answer.AnswerText[0];
            foreach (var option in question.Options)
                if (option.IsCorrect &&
                    option.OptionText.Equals(answerText, StringComparison.OrdinalIgnoreCase))
                {
                    result = true;
                    break;
                }
        }
        else
        {
            foreach (var option in question.Options)
                if (option.IsCorrect &&
                    !answer.AnswerText.Contains(option.OptionText))
                    break;
            result = true;
        }

        return result;
    }

    public async Task<bool> UpdateQuestionAsync(
        Guid questionId,
        UpdateQuestionDto dto)
    {
        var question = await _questionRepo.GetQuestionByIdAsync(questionId);
        if (question is null)
            return false;

        question.QuestionText = dto.QuestionText;
        question.Type = dto.Type;
        question.Points = dto.Points;
        question.OrderIndex = dto.OrderIndex;

        _questionRepo.Update(question);
        await _uow.SaveChangesAsync();

        if (dto.Options.Count > 0) {
            question.Options.Clear();
            foreach (var option in dto.Options)
            {
                question.Options.Add(new QuestionOption
                {
                    QuestionId = question.Id,
                    OptionText = option.OptionText,
                    IsCorrect = option.IsCorrect
                });
            }
            await _uow.SaveChangesAsync();
        }

        return true;
    }

    public async Task<bool> DeleteQuestionAsync(Guid questionId)
    {
        var question = await _questionRepo.GetQuestionByIdAsync(questionId);
        if (question is null)
            return false;

        _questionRepo.Remove(question);
        await _uow.SaveChangesAsync();

        return true;
    }
}
