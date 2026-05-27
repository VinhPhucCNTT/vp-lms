using Backend.Core.Common.Models;
using Backend.Core.Entities.Resources;
using Backend.Core.Entities.Users;

namespace Backend.Core.Entities.Submissions;

public class AssignmentSubmission : BaseEntity, ISoftDeletable
{
    public Guid AssignmentId { get; set; }
    public Guid UserId { get; set; }
    public string? SubmissionText { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Assignment Assignment { get; set; } = default!;
    public User User { get; set; } = default!;
    public AssignmentGrade? Grade { get; set; }
}
