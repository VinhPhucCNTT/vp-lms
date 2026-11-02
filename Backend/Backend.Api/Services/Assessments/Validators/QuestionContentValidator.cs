using System.Text.Json;
using Backend.Api.Core.Common;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Assessments;

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
