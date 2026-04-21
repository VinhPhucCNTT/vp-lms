using Backend.Models.Users;

namespace Backend.Models.Assessments;

public class AssessmentAttempt
{
    public Guid Id { get; set; }

    public Guid AssessmentId { get; set; }

    public Assessment Assessment { get; set; } = default!;

    public Guid StudentId { get; set; }

    public ApplicationUser Student { get; set; } = default!;

    public DateTime StartedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public decimal Score { get; set; }

    public bool IsPassed { get; set; }

    public ICollection<AttemptAnswer> Answers { get; set; } = [];
}
