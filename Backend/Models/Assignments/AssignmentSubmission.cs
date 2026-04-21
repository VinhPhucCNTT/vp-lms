using Backend.Models.Users;

namespace Backend.Models.Assignments;

public class AssignmentSubmission
{
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }

    public Assignment Assignment { get; set; } = default!;

    public Guid StudentId { get; set; }

    public ApplicationUser Student { get; set; } = default!;

    public string? SubmissionText { get; set; }

    public string? FileUrl { get; set; }

    public DateTime SubmittedAt { get; set; }

    public decimal? Grade { get; set; }

    public string? Feedback { get; set; }
}
