using System.Text.Json;
using Backend.Api.Core.Common;
using Backend.Api.Core.Entities.Assessments;
using Backend.Api.Core.Types;

namespace Backend.Api.Services.Assessments.Validators;

public interface IQuestionContentValidator
{
    Result Validate(
        QuestionType type,
        JsonDocument content);
}

public sealed class QuestionContentValidator(
    IEnumerable<IQuestionTypeValidator> validators)
    : IQuestionContentValidator
{
    public Result Validate(
        QuestionType type,
        JsonDocument content)
    {
        var validator = validators
            .FirstOrDefault(x => x.Type == type);

        if (validator is null)
        {
            return Result.Failure(
                QuestionErrors.InvalidContent);
        }

        return validator.Validate(content);
    }
}

public interface IQuestionTypeValidator
{
    QuestionType Type { get; }

    Result Validate(JsonDocument content);
}
