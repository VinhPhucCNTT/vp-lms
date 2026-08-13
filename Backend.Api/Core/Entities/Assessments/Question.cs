using System.Text.Json;
using Backend.Api.Core.Common.Models;

namespace Backend.Api.Core.Entities.Assessments;

public class Question : BaseEntity, ISoftDeletable
{
    public long CategoryId { get; set; }

    public string QuestionType { get; set; } = default!;
    public string Text { get; set; } = default!;
    public JsonDocument QuestionData { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public QuestionCategory Category { get; set; } = default!;
    public ICollection<AssessmentQuestion> AssessmentQuestions { get; set; } = [];
}
