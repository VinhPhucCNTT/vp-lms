using Backend.Core.Common.Models;
using Backend.Core.Entities.Resources;
using Backend.Core.Entities.Users;

namespace Backend.Core.Entities.Submissions;

public class ProblemSubmission : BaseEntity
{
    public long ProblemId { get; set; }
    public long UserId { get; set; }
    public string SubmittedCode { get; set; } = default!;
    public string Language { get; set; } = default!;
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
    public int? ExecutionTimeMs { get; set; }
    public int? MemoryUsedKb { get; set; }
    public int? PassedTestCases { get; set; }
    public int? TotalTestCases { get; set; }

    // Navigation properties
    public CodingProblem Problem { get; set; } = default!;
    public User User { get; set; } = default!;
    public ICollection<ProblemTestResult> TestResults { get; set; } = [];
}

public enum SubmissionStatus
{
    Pending,
    Accepted,
    WrongAnswer,
    RuntimeError,
    TimeLimitExceeded,
    MemoryLimitExceeded,
    CompilationError
}
