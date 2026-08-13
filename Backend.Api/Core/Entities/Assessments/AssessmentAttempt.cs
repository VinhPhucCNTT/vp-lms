using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Content;
using Backend.Api.Core.Entities.Users;

namespace Backend.Api.Core.Entities.Assessments;

public class AssessmentAttempt : BaseEntity, ISoftDeletable
{
    public long AssessmentId { get; set; }
    public long StudentId { get; set; }

    public int AttemptNumber { get; set; }

    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }

    public AssessmentAttemptStatus Status { get; set; }
    public decimal? TotalScore { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Assessment Assessment { get; set; } = default!;
    public User Student { get; set; } = default!;
    public ICollection<AttemptQuestion> Questions { get; set; } = [];
}

public enum AssessmentAttemptStatus
{
    InProgress,
    Submitted,
    Graded
}
