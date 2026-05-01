using Backend.Models.Common;
using Backend.Models.Courses;
using Backend.Models.Submissions;

namespace Backend.Models.Resources;

public class Coding : BaseEntity
{
    public Guid ResourceId { get; set; }
    public string ProblemStatementMarkdown { get; set; } = default!;
    public string? ConstraintsMarkdown { get; set; }
    public string FunctionSignature { get; set; } = default!;
    public string Language { get; set; } = default!;
    public int TimeLimitMs { get; set; } = 1000;
    public int MemoryLimitMb { get; set; } = 256;
    public bool IsPractice { get; set; } = false;

    // Navigation properties
    public ModuleResource Resource { get; set; } = default!;
    public ICollection<CodingTestCase> TestCases { get; set; } = [];
    public ICollection<CodingSubmission> Submissions { get; set; } = [];
}
