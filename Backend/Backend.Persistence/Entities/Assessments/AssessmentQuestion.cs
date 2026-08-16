using System.Text.Json;
using Backend.Persistence.Common;
using Backend.Persistence.Entities.Content;

namespace Backend.Persistence.Entities.Assessments;

public class AssessmentQuestion : BaseEntity
{
    public long AssessmentId { get; set; }
    public long? QuestionId { get; set; }

    public QuestionType QuestionType { get; set; }
    public string Text { get; set; } = "";
    required public JsonDocument QuestionData { get; set; }

    public int OrderIndex { get; set; }
    public decimal Points { get; set; } = 1;

    // Navigation properties
    public Assessment Assessment { get; set; } = default!;
    public Question? Question { get; set; }
    public ICollection<AttemptAnswer> Answers { get; set; } = [];
}
