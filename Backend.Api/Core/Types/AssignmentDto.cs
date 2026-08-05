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
    long ResourceId,
    FileResponse FileInfo
);

public record AssignmentSubmitRequest(
    string? SubmissionText
);

public record AssignmentSubmitResponse(
    string AssignmentId,
    string UserId,
    string? SubmissionText,
    FileResponse[] Files
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
