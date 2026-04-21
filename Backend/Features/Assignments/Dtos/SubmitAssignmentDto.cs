namespace Backend.Features.Assignments.Dtos;

public record SubmitAssignmentDto(
    string SubmissionText,
    string? FileUrl
);
