using Backend.Api.Core.Common.Models;

namespace Backend.Api.Core.Entities.Assessments;

public class AttemptQuestion : BaseEntity
{
    public long AttemptId { get; set; }
    public long QuestionId { get; set; }

    public int OrderIndex { get; set; }
    public decimal Points { get; set; }

    // Navigation properties
    public AssessmentAttempt Attempt { get; set; } = default!;
    public Question Question { get; set; } = default!;

    public AttemptAnswer? Answer { get; set; }
}
