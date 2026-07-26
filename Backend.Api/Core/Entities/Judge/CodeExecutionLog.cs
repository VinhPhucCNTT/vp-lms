using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Content;
using Backend.Api.Core.Entities.Users;

namespace Backend.Api.Core.Entities.Judge;

public class CodeExecutionLog : BaseEntity
{
    public long UserId { get; set; }
    public long? ProblemId { get; set; }
    public string Language { get; set; } = default!;
    public string? CodeSnippet { get; set; }
    public int? ExecutionTimeMs { get; set; }
    public int? MemoryUsedKb { get; set; }
    public string? Status { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = default!;
    public CodingProblem? Problem { get; set; }
}
