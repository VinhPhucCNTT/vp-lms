using Backend.Persistence.Common;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Assignments;

namespace Backend.Persistence.Entities.Content;

public class Assignment : BaseEntity, ISoftDeletable
{
    public long ResourceId { get; set; }

    public string InstructionsMD { get; set; } = default!;
    public SubmissionType SubmissionType { get; set; } = SubmissionType.Both;

    public string[]? AllowedExtensions { get; set; } = [];
    public int MaxFileSizeKb { get; set; } = 10;
    public int? MaxFileCount { get; set; }

    public int? MinTextLength { get; set; }
    public int? MaxTextLength { get; set; }

    public DateTime? OpenDate { get; set; }
    public DateTime? CloseDate { get; set; }
    public string? GradingSchemaJson { get; set; } // JSONB in PostgreSQL

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public CourseResource Resource { get; set; } = default!;
    public ICollection<AssignmentSubmission> Submissions { get; set; } = [];
}

public enum SubmissionType
{
    File,
    Text,
    Both
}
