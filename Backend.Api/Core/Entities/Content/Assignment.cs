using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Courses;
using Backend.Api.Core.Entities.Assignments;

namespace Backend.Api.Core.Entities.Content;

public class Assignment : BaseEntity, ISoftDeletable
{
    public long ResourceId { get; set; }
    public string InstructionsMD { get; set; } = default!;
    public string? AllowedFileTypes { get; set; }
    public int MaxFileSizeKb { get; set; } = 10;
    public int? MaxFileCount { get; set; }
    public SubmissionType SubmissionType { get; set; } = SubmissionType.Both;
    public string? GradingSchemaJson { get; set; } // JSONB in PostgreSQL

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public CourseResource Resource { get; set; } = default!;
    public ICollection<AssignmentSubmission> Submissions { get; set; } = [];
    public ICollection<AssignmentFile> Files { get; set; } = [];
}

public enum SubmissionType
{
    File,
    Text,
    Both
}
