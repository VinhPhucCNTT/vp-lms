using Backend.Persistence.Common;
using Backend.Persistence.Entities.Content;

namespace Backend.Persistence.Entities.Assignments;

public class AssignmentFile : BaseEntity
{
    public long SubmissionId { get; set; }
    public long FileId { get; set; }
    public int OrderIndex { get; set; }

    // Navigation properties
    public AssignmentSubmission Submission { get; set; } = default!;
    public FileAsset File { get; set; } = default!;
}
