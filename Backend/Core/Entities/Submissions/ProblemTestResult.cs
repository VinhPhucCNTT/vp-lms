using Backend.Core.Common.Models;
using Backend.Core.Entities.Resources;

namespace Backend.Core.Entities.Submissions;

public class ProblemTestResult : BaseEntity
{
    public Guid SubmissionId { get; set; }
    public Guid TestCaseId { get; set; }
    public string Status { get; set; } = default!;
    public string? ActualOutput { get; set; }
    public string? ErrorMessage { get; set; }
    public int? ExecutionTimeMs { get; set; }
    public int? MemoryUsedKb { get; set; }

    // Navigation properties
    public ProblemSubmission Submission { get; set; } = default!;
    public ProblemTestCase TestCase { get; set; } = default!;
}
