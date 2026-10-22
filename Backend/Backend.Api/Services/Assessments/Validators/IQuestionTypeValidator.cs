using System.Text.Json;
using Backend.Api.Core.Common;
using Backend.Persistence.Entities.Assessments;

namespace Backend.Api.Services.Assessments.Validators;

public interface IQuestionTypeValidator
{
    QuestionType Type { get; }

    Result Validate(JsonDocument content);
}
