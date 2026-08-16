using Backend.Persistence.Common;

namespace Backend.Persistence.Entities.Content;

public class ProblemTestCase : BaseEntity
{
    public long ProblemId { get; set; }

    public int OrderIndex { get; set; }
    public string Input { get; set; } = default!;
    public string ExpectedOutput { get; set; } = default!;

    // Navigation property
    public CodingProblem Problem { get; set; } = default!;
}
