using Backend.Persistence.Entities.Content;

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

public record AssignmentListInfo(
    SubmissionType SubmissionType,
    DateTime? OpenDate,
    DateTime? CloseDate
);

public record AssignmentRequest(
    ActivityRequestInfo ActivityInfo,
    AssignmentInfo Info
);

public record AssignmentResponse(
    ActivityDetailResponse ActivityInfo,
    AssignmentInfo Info
);

public record StudentAssignmentSummaryResponse(
    AssignmentResponse Assignment,
    CourseResponse Course,
    string Status,
    DateTime? SubmittedAt,
    decimal? Score,
    string? FeedbackText,
    int SubmittedFileCount
);

public record AssignmentFileResponse(
    string ActivityId,
    FileResponse FileInfo
);

public record SubmissionRequest(
    string? SubmissionText
);

public record SubmissionDetailResponse(
    string AssignmentId,
    string UserId,
    string? SubmissionText,
    DateTime? SubmittedOn,
    string Status,
    decimal? Score,
    string? FeedbackText,
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
