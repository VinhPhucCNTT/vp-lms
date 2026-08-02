using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Assessments;

namespace Backend.Api.Core.Entities.Content;

public class AssessmentQuestion : BaseEntity, ISoftDeletable
{
    public long AssessmentId { get; set; }
    public long SectionId { get; set; }

    public int OrderIndex { get; set; }
    public string QuestionType { get; set; } = default!;
    public string QuestionTextMarkdown { get; set; } = default!;
    public string QuestionDataJson { get; set; } = default!; // JSONB column
    // TODO: Implement more advanced scoring (percentage based, etc?)
    public decimal Points { get; set; } = 1;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    
    // Navigation properties
    public Assessment Assessment { get; set; } = default!;
    // public AssessmentSection Section { get; set; } = default!;
    public ICollection<AttemptAnswer> Answers { get; set; } = [];
}
