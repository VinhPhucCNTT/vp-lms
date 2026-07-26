using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Content;

namespace Backend.Api.Core.Entities.Judge;

public class ProblemTestResult : BaseEntity
{
    public long SubmissionId { get; set; }
    public long TestCaseId { get; set; }

    public string Status { get; set; } = default!;
    public string? ActualOutput { get; set; }
    public string? ErrorMessage { get; set; }
    public int? ExecutionTimeMs { get; set; }
    public int? MemoryUsedKb { get; set; }

    // Navigation properties
    public ProblemSubmission Submission { get; set; } = default!;
    public ProblemTestCase TestCase { get; set; } = default!;
}
