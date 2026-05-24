using Backend.Common.Models;
using Backend.Models.Resources;
using Backend.Models.Users;

namespace Backend.Models.Submissions;

public class CodeExecutionLog : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid? ProblemId { get; set; }
    public string Language { get; set; } = default!;
    public string? CodeSnippet { get; set; }
    public int? ExecutionTimeMs { get; set; }
    public int? MemoryUsedKb { get; set; }
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = default!;
    public Problem? Problem { get; set; }
}
