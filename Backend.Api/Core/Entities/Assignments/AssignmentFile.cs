using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Content;

namespace Backend.Api.Core.Entities.Assignments;

public class AssignmentFile : BaseEntity
{
    public long SubmissionId { get; set; }
    public long FileId { get; set; }
    public int OrderIndex { get; set; }

    // Navigation properties
    public AssignmentSubmission Submission { get; set; } = default!;
    public FileAsset File { get; set; } = default!;
}
