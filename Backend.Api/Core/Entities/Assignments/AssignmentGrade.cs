using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Users;

namespace Backend.Api.Core.Entities.Assignments;

public class AssignmentGrade : BaseEntity
{
    public long SubmissionId { get; set; }
    public long GraderId { get; set; }

    public decimal Score { get; set; }
    public string? FeedbackText { get; set; }

    // Navigation properties
    public AssignmentSubmission Submission { get; set; } = default!;
    public User Grader { get; set; } = default!;
}
