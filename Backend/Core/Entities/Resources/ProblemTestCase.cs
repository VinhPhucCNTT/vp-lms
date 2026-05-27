using Backend.Core.Common.Models;

namespace Backend.Core.Entities.Resources;

public class ProblemTestCase : BaseEntity, ISoftDeletable
{
    public Guid ProblemId { get; set; }
    public string InputData { get; set; } = default!;
    public string ExpectedOutput { get; set; } = default!;
    public bool IsSample { get; set; } = false;
    public int OrderIndex { get; set; }
    public string? Explanation { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation property
    public Problem Problem { get; set; } = default!;
}
