using Backend.Persistence.Common;

namespace Backend.Persistence.Entities.Assessments;

public class AttemptQuestion : BaseEntity, ISoftDeletable
{
    public long AttemptId { get; set; }
    public long AssessmentQuestionId { get; set; }

    public int OrderIndex { get; set; }
    public decimal Points { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public bool IsFlagged { get; set; } = false;

    // Navigation properties
    public AssessmentAttempt Attempt { get; set; } = default!;
    public AssessmentQuestion AssessmentQuestion { get; set; } = default!;

    public AttemptAnswer? Answer { get; set; }
}
