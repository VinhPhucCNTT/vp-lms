using Backend.Persistence.Common;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Users;

namespace Backend.Persistence.Entities.Judge;

public class ProblemSubmission : BaseEntity
{
    public long ProblemId { get; set; }
    public long UserId { get; set; }

    public int OrderIndex { get; set; }
    public string SubmittedCode { get; set; } = default!;
    public ProgrammingLanguage Language { get; set; }
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
