namespace Backend.Features.Assessments.Dtos.Submission;

public record QuestionFeedbackDto(
    Guid QuestionId,
    bool IsCorrect,
    decimal AwardedPoints
);
