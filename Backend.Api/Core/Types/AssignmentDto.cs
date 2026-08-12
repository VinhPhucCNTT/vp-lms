using Backend.Api.Core.Entities.Content;

namespace Backend.Api.Core.Types;

public record AssignmentInfo(
    string InstructionsMD,
    SubmissionType SubmissionType,
    string[]? AllowedExtensions,
    int MaxFileSizeKb,
    int? MaxFileCount,
    int? MinTextLength,
    int? MaxTextLength,
    DateTime? OpenDate,
    DateTime? CloseDate,
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

public record AssignmentFileResponse(
    string ResourceId,
    FileResponse FileInfo
);

public record SubmissionRequest(
    string? SubmissionText
);

public record SubmissionDetailResponse(
    string AssignmentId,
    string UserId,
    string? SubmissionText,
    FileResponse[] Files
);

public record SubmissionResponse(
    string AssignmentId,
    string SubmissionId,
    string UserId
);

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
