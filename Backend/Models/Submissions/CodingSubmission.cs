using Backend.Models.Common;
using Backend.Models.Resources;
using Backend.Models.Users;

namespace Backend.Models.Submissions;

public class CodingSubmission : BaseEntity
{
    public Guid ChallengeId { get; set; }
    public Guid UserId { get; set; }
    public string SubmittedCode { get; set; } = default!;
    public string Language { get; set; } = default!;
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Pending;
    public int? ExecutionTimeMs { get; set; }
    public int? MemoryUsedKb { get; set; }
    public int? PassedTestCases { get; set; }
    public int? TotalTestCases { get; set; }

    // Navigation properties
    public Coding Challenge { get; set; } = default!;
    public User User { get; set; } = default!;
    public ICollection<CodingTestResult> TestResults { get; set; } = [];
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
