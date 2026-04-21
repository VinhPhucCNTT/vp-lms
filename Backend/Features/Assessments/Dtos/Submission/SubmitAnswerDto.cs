namespace Backend.Features.Assessments.Dtos.Submission;

public record SubmitAnswerDto(
    Guid QuestionId,
    List<string> AnswerText
);
