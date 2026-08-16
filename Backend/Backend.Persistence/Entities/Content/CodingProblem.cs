using Backend.Persistence.Common;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Judge;

namespace Backend.Persistence.Entities.Content;

public class CodingProblem : BaseEntity, ISoftDeletable
{
    public long ResourceId { get; set; }
    public string ProblemStatementMarkdown { get; set; } = default!;
    public string? ConstraintsMarkdown { get; set; }
    public string FunctionSignature { get; set; } = default!;
    public string Language { get; set; } = default!;
    public int TimeLimitMs { get; set; } = 1000;
    public int MemoryLimitKb { get; set; } = 256;
    public bool IsPractice { get; set; } = false;
    public ProblemDifficulty Difficulty { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public CourseResource Resource { get; set; } = default!;
    public ICollection<ProblemTestCase> TestCases { get; set; } = [];
    public ICollection<ProblemSubmission> Submissions { get; set; } = [];
}

public enum ProblemDifficulty
{
    Unspecified = 0,
    Easy = 1,
    Medium = 2,
    Hard = 3,
};
