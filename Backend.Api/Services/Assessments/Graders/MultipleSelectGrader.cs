using System.Text.Json;
using Backend.Api.Core.Common;
using Backend.Api.Core.Entities.Assessments;
using Backend.Api.Core.Types;

namespace Backend.Api.Services.Assessments.Graders;

public sealed class MultipleSelectGrader
    : IQuestionGrader
{
    public QuestionType Type =>
        QuestionType.MultipleSelect;

    public Result<GradeResult> Grade(
        AssessmentQuestion question,
        AttemptAnswer? answer)
    {
        var content =
            question.QuestionData.Deserialize<MultipleSelectQuestion>();

        if (content is null)
            return Result<GradeResult>.Failure(
                QuestionErrors.InvalidContent);

        if (answer is null)
        {
            return Result<GradeResult>.Success(
                new GradeResult(0, false));
        }

        var answerModel =
            answer.AnswerData.Deserialize<MultipleSelectAnswer>();

        if (answerModel is null)
        {
            return Result<GradeResult>.Failure(
                new Error(
                    "answer.invalid",
                    "Invalid multiple-select answer."));
        }

        var selected = answerModel.SelectedOptionIds
            .ToHashSet();

        var correct = content.Options
            .Where(x => x.IsCorrect)
            .Select(x => x.Id)
            .ToHashSet();

        var isCorrect = selected.SetEquals(correct);

        return Result<GradeResult>.Success(
            new GradeResult(
                isCorrect ? question.Points : 0,
                isCorrect));
    }
}

public sealed class MultipleSelectAnswer
{
    public List<string> SelectedOptionIds { get; set; } = [];
}
