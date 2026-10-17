using Backend.Persistence.Common;
using Backend.Persistence.Entities.Content;

namespace Backend.Persistence.Entities.Judge;

public class ProblemTestResult : BaseEntity
{
    public long SubmissionId { get; set; }
    public long TestCaseId { get; set; }

    public bool IsAccepted { get; set; }

    public string? RuntimeOutput { get; set; } // stdout, null means same as test case
    public string? RuntimeError { get; set; } // stderr

    public double? ExecutionTimeMs { get; set; }
    public int? MemoryUsedKb { get; set; }

    // Navigation properties
    public ProblemSubmission Submission { get; set; } = default!;
    public ProblemTestCase TestCase { get; set; } = default!;
}
