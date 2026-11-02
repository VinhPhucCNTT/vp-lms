using Backend.Persistence.Common;

namespace Backend.Persistence.Entities.Content;

public class ProblemTestCase : BaseEntity, ISoftDeletable
{
    public long ProblemId { get; set; }

    public int OrderIndex { get; set; }
    public string Input { get; set; } = default!;
    public string ExpectedOutput { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation property
    public CodingProblem Problem { get; set; } = default!;
}
