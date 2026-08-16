using Backend.Persistence.Common;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Users;

namespace Backend.Persistence.Entities.Assessments;

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
