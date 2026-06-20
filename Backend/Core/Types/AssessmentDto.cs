namespace Backend.Core.Types;

public record AssessmentRequest(
    string? InstructionsMarkdown,
    int? TimeLimitMinutes,
    int MaxAttempts,
    bool ShuffleQuestions,
    bool ShowResults,
    string? GradingSchemaJson
);

public record AssessmentResponseDto(
    string AssessmentSqid,
    string? InstructionsMarkdown,
    int? TimeLimitMinutes,
    int MaxAttempts,
    bool ShuffleQuestions,
    bool ShowResults,
    string? GradingSchemaJson
);

public record QuestionRequest(
    string? QuestionSqid,
    string QuestionType,
    string QuestionTextMarkdown,
    decimal Points,
    int OrderIndex,
    string QuestionDataJson
);

public record QuestionResponse(
    string QuestionSqid,
    string QuestionType,
    string QuestionTextMarkdown,
    decimal Points,
    int OrderIndex,
    string QuestionDataJson
);

public record AssessmentAttemptRequest(
    DateTime StartedAt,
    DateTime? SubmittedAt,
    decimal? TotalScore,
    bool? IsPassed,
    int AttemptNumber
);

public record AssessmentAttemptResponse(
    string AssessmentAttemptSqid,
    DateTime StartedAt,
    DateTime? SubmittedAt,
    decimal? TotalScore,
    bool? IsPassed,
    int AttemptNumber
);
