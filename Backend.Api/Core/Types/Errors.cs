using Backend.Api.Core.Common;

namespace Backend.Api.Core.Types;

public static class AssessmentErrors
{
    public static readonly Error NotFound =
        new("assessment.not_found", "Assessment was not found.");

    public static readonly Error Forbidden =
        new("assessment.forbidden", "You do not have permission to perform this operation.");

    public static readonly Error InvalidState =
        new("assessment.invalid_state", "The assessment is in an invalid state.");

    public static readonly Error HasAttempts =
        new(
            "assessment.has_attempts",
            "Questions cannot be modified after students have started an attempt.");
}

public static class QuestionErrors
{
    public static readonly Error NotFound =
        new("question.not_found", "Question was not found.");

    public static readonly Error Forbidden =
        new("question.forbidden", "You do not have permission to modify this question.");

    public static readonly Error InvalidContent =
        new("question.invalid_content", "The question content is invalid.");

    public static readonly Error HasAttempts =
        new(
            "question.has_attempts",
            "This question has already been used by an assessment attempt.");
}
