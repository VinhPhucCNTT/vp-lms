namespace Backend.Features.Assessments.Dtos.Submission;

public record AssessmentResultDto(
    decimal Score,
    bool Passed
);
