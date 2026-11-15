using Backend.Persistence.Common;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Learning;

namespace Backend.Persistence.Entities.Courses;

public enum ActivityType
{
    Lesson,
    Assignment,
    Assessment,
    Problem
}

public class CourseActivity : BaseEntity, ISoftDeletable
{
    public long ModuleId { get; set; }

    public ActivityType Type { get; set; }
    public required string Title { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPublished { get; set; } = false;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    public Lesson? Lesson { get; set; }
    public Assignment? Assignment { get; set; }
    public Assessment? Assessment { get; set; }
    public CodingProblem? Problem { get; set; }

    // Navigation properties
    public CourseModule Module { get; set; } = default!;
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<ActivityProgress> Progress { get; set; } = [];
}
