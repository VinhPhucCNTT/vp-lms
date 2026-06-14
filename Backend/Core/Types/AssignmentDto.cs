using Backend.Core.Entities.Resources;

namespace Backend.Core.Types;

public record AssignmentRequest(
    string InstructionsMarkdown,
    string? AllowedFileTypes,
    int MaxFileSizeKb,
    SubmissionType SubmissionType,
    string? GradingSchemaJson
);

public record AssignmentResponse(
    string AssignmentSqid,
    string InstructionsMarkdown,
    string? AllowedFileTypes,
    int MaxFileSizeKb,
    SubmissionType SubmissionType,
    string? GradingSchemaJson
);

public record SubmissionRequest(
    string? SubmissionText,
    string? FileUrl,
    string? FileName
);

public record SubmissionResponse(
    string AssignmentSqid,
    string UserSqid,
    string? SubmissionText,
    string? FileUrl,
    string? FileName
);

public record AssignmentGradeRequest(
    decimal Score,
    string? FeedbackText
);

public record AssignmentGradeResponse(
    string SubmissionSqid,
    string GraderSqid,
    decimal Score,
    string? FeedbackText
);

public record AssignmentStatsResponse(
    int SubmissionCount,
    int GradedSubmissionCount
);
