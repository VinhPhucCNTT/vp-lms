using Backend.Api.Core.Common;

namespace Backend.Api.Core.Types;

public static class UserErrors
{
    public static readonly Error Upload =
        new("user.upload", "Failed to upload avatar.");

    public static readonly Error NotFound =
        new("user.not_found", "User not found.");

    public static readonly Error Forbidden =
        new("user.forbidden", "You do not have permission to perform this operation.");
}

public static class ResourceErrors
{
    public static readonly Error NotFound =
        new("resource.not_found", "Learning resource not found.");

    public static readonly Error Forbidden =
        new("resource.forbidden", "You do not have permission to perform this operation.");
}

public static class AssessmentErrors
{
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

public static class SubmissionErrors
{
    public static readonly Error NotFound =
        new("submission.not_found", "Submission not found.");

    public static readonly Error Internal =
        new("submission.internal", "Internal server error.");

    public static readonly Error Upload =
        new("submission.upload", "Failed to upload submission file.");
}

public static class JudgeSubmissionErrors
{
    public static readonly Error Internal =
        new("judge_submission.internal", "Internal server error.");

    public static readonly Error NotFound =
        new("judge_submission.not_found", "Judge submission not found.");
}

public static class CompilerErrors
{
    public static readonly Error Internal =
        new("compiler.internal", "Internal server error.");

    public static readonly Error NotFound =
        new("compiler.not_found", "Compiler not found.");
}
