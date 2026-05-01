using Backend.Models.Common;
using Backend.Models.Courses;
using Backend.Models.Submissions;

namespace Backend.Models.Resources;

public class Assessment : BaseEntity
{
    public Guid ResourceId { get; set; }
    public string? InstructionsMarkdown { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int MaxAttempts { get; set; } = 1;
    public bool ShuffleQuestions { get; set; } = false;
    public bool ShowResults { get; set; } = true;
    public decimal? PassingScore { get; set; }

    // Navigation properties
    public ModuleResource Resource { get; set; } = default!;
    public ICollection<AssessmentQuestion> Questions { get; set; } = [];
    public ICollection<AssessmentAttempt> Attempts { get; set; } = [];
}
