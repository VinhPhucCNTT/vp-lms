using System.Text.Json;
using Backend.Persistence.Common;

namespace Backend.Persistence.Entities.Assessments;

public class Question : BaseEntity, ISoftDeletable
{
    public long QuestionBankId { get; set; }

    public QuestionType QuestionType { get; set; }
    public string Text { get; set; } = default!;
    public JsonDocument QuestionData { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public QuestionBank QuestionBank { get; set; } = default!;
    public ICollection<AssessmentQuestion> AssessmentQuestions { get; set; } = [];
}

public enum QuestionType
{
    MultipleChoice,
    MultipleSelect,
    TrueFalse,
    ShortAnswer,
    DragAndDrop,
    Matching,
    Ordering,
    Coding
}

