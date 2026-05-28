using Backend.Core.Common.Models;
using Backend.Core.Entities.Courses;
using Backend.Core.Entities.Submissions;

namespace Backend.Core.Entities.Resources;

public class Assignment : BaseEntity, ISoftDeletable
{
    public long ResourceId { get; set; }
    public string InstructionsMarkdown { get; set; } = default!;
    public decimal? MaxScore { get; set; }
    public string? AllowedFileTypes { get; set; }
    public int MaxFileSizeMb { get; set; } = 10;
    public SubmissionType SubmissionType { get; set; } = SubmissionType.Both;
    public string? GradingSchemaJson { get; set; } // JSONB in PostgreSQL

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ModuleResource Resource { get; set; } = default!;
    public ICollection<AssignmentSubmission> Submissions { get; set; } = [];
}

public enum SubmissionType
{
    File,
    Text,
    Both
}
