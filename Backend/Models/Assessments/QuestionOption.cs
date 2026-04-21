namespace Backend.Models.Assessments;

public class QuestionOption
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public AssessmentQuestion Question { get; set; } = default!;

    public string OptionText { get; set; } = default!;

    public bool IsCorrect { get; set; }
}
