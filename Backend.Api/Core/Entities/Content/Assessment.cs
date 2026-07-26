using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Assessments;
using Backend.Api.Core.Entities.Courses;

namespace Backend.Api.Core.Entities.Content;

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
    public CourseResource Resource { get; set; } = default!;
    public ICollection<AssessmentQuestion> Questions { get; set; } = [];
    public ICollection<AssessmentAttempt> Attempts { get; set; } = [];
}
