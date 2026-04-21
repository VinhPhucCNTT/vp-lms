using Backend.Models.Enums;

namespace Backend.Models.Assessments;

public class AssessmentQuestion
{
    public Guid Id { get; set; }

    public Guid AssessmentId { get; set; }

    public Assessment Assessment { get; set; } = default!;

    public string QuestionText { get; set; } = default!;

    public QuestionType Type { get; set; }

    public decimal Points { get; set; }

    public int OrderIndex { get; set; }

    public ICollection<QuestionOption> Options { get; set; } = [];
}
