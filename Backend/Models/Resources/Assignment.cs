using Backend.Models.Common;
using Backend.Models.Courses;
using Backend.Models.Submissions;

namespace Backend.Models.Resources;

public class Assignment : BaseEntity
{
    public Guid ResourceId { get; set; }
    public string InstructionsMarkdown { get; set; } = default!;
    public decimal? MaxScore { get; set; }
    public string? AllowedFileTypes { get; set; }
    public int MaxFileSizeMb { get; set; } = 10;
    public SubmissionType SubmissionType { get; set; } = SubmissionType.Both;
    public string? GradingSchemaJson { get; set; } // JSONB in PostgreSQL

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
