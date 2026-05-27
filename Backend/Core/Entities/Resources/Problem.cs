using Backend.Core.Common.Models;
using Backend.Core.Entities.Courses;
using Backend.Core.Entities.Submissions;

namespace Backend.Core.Entities.Resources;

public class Problem : BaseEntity, ISoftDeletable
{
    public Guid ResourceId { get; set; }
    public string ProblemStatementMarkdown { get; set; } = default!;
    public string? ConstraintsMarkdown { get; set; }
    public string FunctionSignature { get; set; } = default!;
    public string Language { get; set; } = default!;
    public int TimeLimitMs { get; set; } = 1000;
    public int MemoryLimitMb { get; set; } = 256;
    public bool IsPractice { get; set; } = false;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ModuleResource Resource { get; set; } = default!;
    public ICollection<ProblemTestCase> TestCases { get; set; } = [];
    public ICollection<ProblemSubmission> Submissions { get; set; } = [];
}
