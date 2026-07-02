using Backend.Core.Entities.Resources;

namespace Backend.Core.Types;

public record AssignmentRequest(
    string InstructionsMarkdown,
    string? AllowedFileTypes,
    int MaxFileSizeKb,
    int? MaxAttempt,
    SubmissionType SubmissionType,
    string? GradingSchemaJson
);

public record AssignmentResponse(
    string Id,
    string InstructionsMarkdown,
    string? AllowedFileTypes,
    int MaxFileSizeKb,
    int? MaxAttempt,
    SubmissionType SubmissionType,
    string? GradingSchemaJson
) : IEntityResponse;

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
