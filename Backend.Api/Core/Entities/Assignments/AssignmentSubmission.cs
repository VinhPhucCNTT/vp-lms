using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Content;
using Backend.Api.Core.Entities.Users;

namespace Backend.Api.Core.Entities.Assignments;

public class AssignmentSubmission : BaseEntity, ISoftDeletable
{
    public long AssignmentId { get; set; }
    public long UserId { get; set; }

    public string? SubmissionText { get; set; }

    public DateTime? SubmittedOn { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Assignment Assignment { get; set; } = default!;
    public User User { get; set; } = default!;
    public AssignmentGrade? Grade { get; set; }
    public ICollection<AssignmentFile> Files { get; set; } = [];
}
