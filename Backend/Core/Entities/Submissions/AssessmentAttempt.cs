using Backend.Core.Common.Models;
using Backend.Core.Entities.Resources;
using Backend.Core.Entities.Users;

namespace Backend.Core.Entities.Submissions;

public class AssessmentAttempt : BaseEntity, ISoftDeletable
{
    public long AssessmentId { get; set; }
    public long UserId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public decimal? TotalScore { get; set; }
    public bool? IsPassed { get; set; }
    public int AttemptNumber { get; set; } = 1;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Assessment Assessment { get; set; } = default!;
    public User User { get; set; } = default!;
    public ICollection<AttemptAnswer> Answers { get; set; } = [];
}
