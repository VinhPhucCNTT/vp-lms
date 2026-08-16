using System.Text.Json;
using Backend.Api.Core.Common;
using Backend.Persistence.Entities.Assessments;
using Backend.Api.Core.Types;

namespace Backend.Api.Services.Assessments.Graders;

public sealed class TrueFalseGrader
    : IQuestionGrader
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public QuestionType Type =>
        QuestionType.TrueFalse;

    public Result<GradeResult> Grade(
        AssessmentQuestion question,
        AttemptAnswer? answer)
    {
        var content =
            question.QuestionData.Deserialize<TrueFalseQuestion>(JsonOptions);

        if (content is null)
            return Result<GradeResult>.Failure(
                QuestionErrors.InvalidContent);

        if (answer is null)
        {
            return Result<GradeResult>.Success(
                new GradeResult(0, false));
        }

        var model =
            answer.AnswerData.Deserialize<TrueFalseAnswer>(JsonOptions);

        if (model is null)
        {
            return Result<GradeResult>.Failure(
                new Error(
                    "answer.invalid",
                    "Invalid true/false answer."));
        }

        var correct =
            model.Value == content.CorrectAnswer;

        return Result<GradeResult>.Success(
            new GradeResult(
                correct ? question.Points : 0,
                correct));
    }
}

public sealed class TrueFalseAnswer
{
    public bool Value { get; set; }
}
