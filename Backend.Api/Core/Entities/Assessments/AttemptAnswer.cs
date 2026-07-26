using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Content;
using Backend.Api.Core.Entities.Users;

namespace Backend.Api.Core.Entities.Assessments;

public class AttemptAnswer : BaseEntity
{
    public long AttemptId { get; set; }
    public long QuestionId { get; set; }

    public string ResponseDataJson { get; set; } = default!; // JSONB column
    public decimal? Score { get; set; }
    // TODO: ????
    public bool? IsCorrect { get; set; }
    public long? GradedByUserId { get; set; }
    public DateTime? GradedAt { get; set; }
    public string? FeedbackText { get; set; }

    // Navigation properties
    public AssessmentAttempt Attempt { get; set; } = default!;
    public AssessmentQuestion Question { get; set; } = default!;
    public User? GradedByUser { get; set; }
}
