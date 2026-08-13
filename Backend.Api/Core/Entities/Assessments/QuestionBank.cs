using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Courses;

namespace Backend.Api.Core.Entities.Assessments;

public class QuestionBank : BaseEntity, ISoftDeletable
{
    public long CourseId { get; set; }

    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<QuestionCategory> Categories { get; set; } = [];
    public Course Course { get; set; } = default!;
}
