namespace Backend.Models.Assessments;

public class AttemptAnswer
{
    public Guid Id { get; set; }

    public Guid AttemptId { get; set; }

    public AssessmentAttempt Attempt { get; set; } = default!;

    public Guid QuestionId { get; set; }

    public AssessmentQuestion Question { get; set; } = default!;

    public string AnswerText { get; set; } = default!;

    public bool IsCorrect { get; set; }

    public decimal AwardedPoints { get; set; }
}
