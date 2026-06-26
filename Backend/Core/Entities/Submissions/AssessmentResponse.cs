using Backend.Core.Common.Models;
using Backend.Core.Entities.Resources;
using Backend.Core.Entities.Users;

namespace Backend.Core.Entities.Submissions;

public class AttemptAnswer : BaseEntity
{
    public long AttemptId { get; set; }
    public long QuestionId { get; set; }
    public string ResponseDataJson { get; set; } = default!; // JSONB column
    public decimal? Score { get; set; }
    public bool? IsCorrect { get; set; }
    public Guid? GradedByUserId { get; set; }
    public DateTime? GradedAt { get; set; }
    public string? FeedbackText { get; set; }

    // Navigation properties
    public AssessmentAttempt Attempt { get; set; } = default!;
    public AssessmentQuestion Question { get; set; } = default!;
    public User? GradedByUser { get; set; }
}
