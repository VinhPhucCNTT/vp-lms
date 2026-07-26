using Backend.Api.Core.Common.Models;

namespace Backend.Api.Core.Entities.Content;

public class ProblemTestCase : BaseEntity, ISoftDeletable
{
    public long ProblemId { get; set; }

    public string InputData { get; set; } = default!;
    public string ExpectedOutput { get; set; } = default!;
    public bool IsSample { get; set; } = false;
    public int OrderIndex { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation property
    public CodingProblem Problem { get; set; } = default!;
}
