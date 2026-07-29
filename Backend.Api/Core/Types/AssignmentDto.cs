using Backend.Api.Core.Entities.Content;

namespace Backend.Api.Core.Types;

public record AssignmentInfo(
    string InstructionsMD,
    string? AllowedFileTypes,
    int MaxFileSizeKb,
    int? MaxAttempt,
    SubmissionType SubmissionType,
    string? GradingSchemaJson
);

public record AssignmentRequest(
    ResourceRequestInfo ResourceInfo,
    AssignmentInfo Info
);

public record AssignmentResponse(
    ResourceDetailResponse ResourceInfo,
    AssignmentInfo Info
);

public record SubmissionRequest(
    string? SubmissionText,
    string? FileUrl,
    string? FileName
);

public record SubmissionResponse(
    string Id,
    string AssignmentId,
    string UserId,
    string? SubmissionText,
    string? FileUrl,
    string? FileName,
    int AttemptNumber
) : IEntityResponse;

public record AssignmentGradeRequest(
    decimal Score,
    string? FeedbackText
);

public record AssignmentGradeResponse(
    string Id,
    string SubmissionId,
    string GraderId,
    decimal Score,
    string? FeedbackText
) : IEntityResponse;

public record AssignmentStatsResponse(
    int SubmissionCount,
    int GradedSubmissionCount
);
