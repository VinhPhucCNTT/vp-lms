using Backend.Models.Common;
using Backend.Models.Resources;
using Backend.Models.Users;

namespace Backend.Models.Submissions;

public class AssignmentSubmission : BaseEntity
{
    public Guid AssignmentId { get; set; }
    public Guid UserId { get; set; }
    public string? SubmissionText { get; set; }
    public string? FileUrl { get; set; }
    public string? FileName { get; set; }
    public bool IsLate { get; set; } = false;

    // Navigation properties
    public Assignment Assignment { get; set; } = default!;
    public User User { get; set; } = default!;
    public AssignmentGrade? Grade { get; set; }
}
