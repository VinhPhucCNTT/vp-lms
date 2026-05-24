using Backend.Common.Models;
using Backend.Models.Resources;
using Backend.Models.Users;

namespace Backend.Models.Submissions;

public class AssessmentResponse : BaseEntity
{
    public Guid AttemptId { get; set; }
    public Guid QuestionId { get; set; }
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
