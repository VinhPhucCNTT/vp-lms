namespace Backend.Api.Core.Types;

public record AssessmentInfo(
    string? InstructionsMarkdown,
    int? TimeLimitMinutes,
    int MaxAttempts,
    bool ShuffleQuestions,
    bool ShowResults,
    string? GradingSchemaJson
);

public record AssessmentRequest(
    ResourceRequestInfo ResourceInfo,
    AssessmentInfo Info
);

public record AssessmentResponse(
    ResourceDetailResponse ResourceInfo,
    AssessmentInfo Info
);

public record AssessmentListResponse(
    ResourceResponse ResourceInfo,
    AssessmentInfo Info
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

public record AssessmentAttemptResponse(
    string AssessmentAttemptSqid,
    DateTime StartedAt,
    DateTime? SubmittedAt,
    decimal? TotalScore,
    bool? IsPassed,
    int AttemptNumber
);
