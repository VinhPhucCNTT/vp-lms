using Backend.Core.Common.Models;
using Backend.Core.Entities.Submissions;

namespace Backend.Core.Entities.Resources;

public class AssessmentQuestion : BaseEntity, ISoftDeletable
{
    public long AssessmentId { get; set; }
    public string QuestionType { get; set; } = default!;
    public string QuestionTextMarkdown { get; set; } = default!;
    // TODO: Implement more advanced scoring (percentage based, etc?)
    public decimal Points { get; set; } = 1;
    public int OrderIndex { get; set; }
    public string QuestionDataJson { get; set; } = default!; // JSONB column

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public Assessment Assessment { get; set; } = default!;
    public ICollection<AssessmentResponse> Responses { get; set; } = [];
}
