using Backend.Models.Common;
using Backend.Models.Resources;
using Backend.Models.Users;

namespace Backend.Models.Submissions;

public class AssessmentAttempt : BaseEntity
{
    public Guid AssessmentId { get; set; }
    public Guid UserId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public decimal? TotalScore { get; set; }
    public bool? IsPassed { get; set; }
    public int AttemptNumber { get; set; } = 1;

    // Navigation properties
    public Assessment Assessment { get; set; } = default!;
    public User User { get; set; } = default!;
    public ICollection<AssessmentResponse> Responses { get; set; } = [];
}
