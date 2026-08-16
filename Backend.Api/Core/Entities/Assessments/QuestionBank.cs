using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Courses;
using Backend.Api.Core.Entities.Users;

namespace Backend.Api.Core.Entities.Assessments;

public class QuestionBank : BaseEntity, ISoftDeletable
{
    public long CourseId { get; set; }
    public long OwnerId { get; set; }

    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<Question> Questions { get; set; } = [];
    public Course Course { get; set; } = default!;
    public User User { get; set; } = default!;
}
