using Backend.Core.Common.Models;
using Backend.Core.Entities.Courses;
using Backend.Core.Entities.Submissions;

namespace Backend.Core.Entities.Resources;

public class Assessment : BaseEntity, ISoftDeletable
{
    public long ResourceId { get; set; }
    public string? InstructionsMarkdown { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int MaxAttempts { get; set; } = 1;
    public bool ShuffleQuestions { get; set; } = false;
    public bool ShowResults { get; set; } = true;
    public string? GradingSchemaJson { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ModuleResource Resource { get; set; } = default!;
    public ICollection<AssessmentQuestion> Questions { get; set; } = [];
    public ICollection<AssessmentAttempt> Attempts { get; set; } = [];
}
