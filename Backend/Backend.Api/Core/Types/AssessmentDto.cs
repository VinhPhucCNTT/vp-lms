using System.Text.Json;
using Backend.Persistence.Entities.Assessments;

namespace Backend.Api.Core.Types;

public record AssessmentInfo(
    string? Description,
    int? TimeLimitMinutes,
    int MaxAttempts,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    bool ShowResults
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
    AssessmentInfo Info,
    int QuestionCount,
    int AttemptsUsed,
    decimal? BestScore,
    decimal BestMaxScore,
    string? LatestAttemptStatus,
    string? LatestAttemptSqid
);

public record AssessmentQuestionInfo(
    QuestionType QuestionType,
    string Text,
    JsonDocument QuestionData,
    int OrderIndex,
    decimal Points
);

public record QuestionRequest(
    string QuestionType,
    string Text,
    JsonDocument QuestionData
);

public record QuestionResponse(
    string Id,
    string BankId,
    string QuestionType,
    string Text,
    JsonDocument QuestionData
) : IEntityResponse;

public record AssessmentAttemptResponse(
    string AssessmentAttemptSqid,
    DateTime StartedAt,
    DateTime? SubmittedAt,
    decimal? TotalScore,
    bool? IsPassed,
    int AttemptNumber
);

public record AssessmentAttemptQuestionResponse(
    string AttemptQuestionSqid,
    string QuestionType,
    string Text,
    JsonDocument QuestionData,
    int OrderIndex,
    decimal Points,
    bool IsFlagged,
    JsonDocument? AnswerData,
    DateTime? AnsweredAt
);

public record AssessmentAttemptDetailResponse(
    string AssessmentAttemptSqid,
    DateTime StartedAt,
    DateTime? SubmittedAt,
    decimal? TotalScore,
    decimal MaxScore,
    bool? IsPassed,
    int AttemptNumber,
    string Status,
    IReadOnlyList<AssessmentAttemptQuestionResponse> Questions
);

public record SaveAttemptAnswerRequest(JsonDocument AnswerData);

public record QuestionBankInfo(
    string Name,
    string? Description
);
