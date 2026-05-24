using Backend.Common.Models;
using Backend.Models.Submissions;

namespace Backend.Models.Resources;

public class AssessmentQuestion : BaseEntity, ISoftDeletable
{
    public Guid AssessmentId { get; set; }
    public string QuestionType { get; set; } = default!;
    public string QuestionTextMarkdown { get; set; } = default!;
    public decimal Points { get; set; } = 1;
    public int OrderIndex { get; set; }
    public string QuestionDataJson { get; set; } = default!; // JSONB column

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public Assessment Assessment { get; set; } = default!;
    public ICollection<AssessmentResponse> Responses { get; set; } = [];
}
