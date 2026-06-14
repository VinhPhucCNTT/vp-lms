using Backend.Core.Common.Models;
using Backend.Core.Entities.Users;

namespace Backend.Core.Entities.Submissions;

public class AssignmentGrade : BaseEntity
{
    public long SubmissionId { get; set; }
    public long GraderId { get; set; }
    public decimal Score { get; set; }
    public string? FeedbackText { get; set; }
    public bool CanResubmit { get; set; } = false; // TODO: Remove?

    // Navigation properties
    public AssignmentSubmission Submission { get; set; } = default!;
    public User Grader { get; set; } = default!;
}
