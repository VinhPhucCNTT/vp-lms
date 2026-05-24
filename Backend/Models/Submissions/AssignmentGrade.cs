using Backend.Common.Models;
using Backend.Models.Users;

namespace Backend.Models.Submissions;

public class AssignmentGrade : BaseEntity
{
    public Guid SubmissionId { get; set; }
    public Guid GraderId { get; set; }
    public decimal? Score { get; set; }
    public string? FeedbackText { get; set; }
    public bool CanResubmit { get; set; } = false;

    // Navigation properties
    public AssignmentSubmission Submission { get; set; } = default!;
    public User Grader { get; set; } = default!;
}
