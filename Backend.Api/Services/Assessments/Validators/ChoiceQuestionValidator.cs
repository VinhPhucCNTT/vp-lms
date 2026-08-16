using System.Text.Json;
using Backend.Api.Core.Common;
using Backend.Api.Core.Entities.Assessments;
using Backend.Api.Core.Types;

namespace Backend.Api.Services.Assessments.Validators;

public sealed class ChoiceQuestionValidator
    : IQuestionTypeValidator
{
    public QuestionType Type =>
        QuestionType.MultipleChoice;

    public Result Validate(JsonDocument content)
    {
        var model = content.Deserialize<MultipleChoiceQuestion>();

        if (model is null)
            return Result.Failure(
                QuestionErrors.InvalidContent);

        if (model.Options.Count < 2)
        {
            return Result.Failure(
                new Error(
                    "question.options_required",
                    "At least two options are required."));
        }

        if (model.Options.Count(x => x.IsCorrect) != 1)
        {
            return Result.Failure(
                new Error(
                    "question.correct_option_count",
                    "A multiple-choice question must have exactly one correct option."));
        }

        return Result.Success();
    }
}
