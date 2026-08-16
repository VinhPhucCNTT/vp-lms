using System.Text.Json;
using Backend.Api.Core.Entities.Assessments;

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
    AssessmentInfo Info
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

public record QuestionBankInfo(
    string Name,
    string? Description
);
