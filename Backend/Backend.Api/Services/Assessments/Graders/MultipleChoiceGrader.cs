using System.Text.Json;
using Backend.Api.Core.Common;
using Backend.Persistence.Entities.Assessments;
using Backend.Api.Core.Types;

namespace Backend.Api.Services.Assessments.Graders;

public sealed class MultipleChoiceGrader
    : IQuestionGrader
{
    public QuestionType Type =>
        QuestionType.MultipleChoice;

    public Result<GradeResult> Grade(
        AssessmentQuestion question,
        AttemptAnswer? answer)
    {
        var content =
            question.QuestionData!.Deserialize<MultipleChoiceQuestion>();

        if (content is null)
            return Result<GradeResult>.Failure(
                QuestionErrors.InvalidContent);

        if (answer is null)
        {
            return Result<GradeResult>.Success(
                new GradeResult(0, false));
        }

        var answerModel =
            answer.AnswerData!.Deserialize<MultipleChoiceAnswer>();

        if (answerModel is null)
        {
            return Result<GradeResult>.Failure(
                new Error(
                    "answer.invalid",
                    "Invalid multiple-choice answer."));
        }

        var correct = content.Options
            .FirstOrDefault(x => x.IsCorrect);

        if (correct is null)
        {
            return Result<GradeResult>.Failure(
                QuestionErrors.InvalidContent);
        }

        var isCorrect =
            correct.Id == answerModel.SelectedOptionId;

        return Result<GradeResult>.Success(
            new GradeResult(
                isCorrect ? question.Points : 0,
                isCorrect));
    }
}

public sealed class MultipleChoiceAnswer
{
    public string? SelectedOptionId { get; set; }
}
