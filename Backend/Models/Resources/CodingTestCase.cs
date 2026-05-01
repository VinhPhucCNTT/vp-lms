using Backend.Models.Common;

namespace Backend.Models.Resources;

public class CodingTestCase : BaseEntity
{
    public Guid ChallengeId { get; set; }
    public string InputData { get; set; } = default!;
    public string ExpectedOutput { get; set; } = default!;
    public bool IsSample { get; set; } = false;
    public int OrderIndex { get; set; }
    public string? Explanation { get; set; }

    // Navigation property
    public Coding Challenge { get; set; } = default!;
}
