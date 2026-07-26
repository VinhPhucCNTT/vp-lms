using Backend.Api.Core.Common.Models;

namespace Backend.Api.Core.Entities.Content;

public class AssessmentSection : BaseEntity, ISoftDeletable
{
    public long AssessmentId { get; set; }
    public string Name { get; set; } = "";

    public int OrderIndex { get; set; }
    public bool ShuffleQuestionOrder { get; set; } = false;
    public int? QuestionToDraw { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Assessment Assessment { get; set; } = default!;
    public ICollection<AssessmentQuestion> Questions { get; set; } = [];
}
