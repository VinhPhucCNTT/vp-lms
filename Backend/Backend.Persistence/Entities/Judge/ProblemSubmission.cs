using Backend.Persistence.Common;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Users;

namespace Backend.Persistence.Entities.Judge;

public class ProblemSubmission : BaseEntity
{
    public long ProblemId { get; set; }
    public long UserId { get; set; }
    public long CompilerId { get; set; }

    public int OrderIndex { get; set; }
    public string SubmittedCode { get; set; } = default!;
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Queued;
    public string? CompileError { get; set; }

    // Navigation properties
    public CodingProblem Problem { get; set; } = default!;
    public User User { get; set; } = default!;
    public Compiler Compiler { get; set; } = default!;
    public ICollection<ProblemTestResult> TestResults { get; set; } = [];
}

public enum SubmissionStatus
{
    Queued,
    Running,
    Accepted,

    WrongAnswer,
    CompilationError,
    RuntimeError,
    TimeLimitExceeded,
    SystemError
}

public enum ProgrammingLanguage
{
    C,
    Cpp,
    Python,
    Java,
    TypeScript,
    CSharp
}
