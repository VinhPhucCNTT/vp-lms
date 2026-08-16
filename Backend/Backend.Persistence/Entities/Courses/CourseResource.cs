using Backend.Persistence.Common;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Learning;

namespace Backend.Persistence.Entities.Courses;

public enum ResourceType
{
    Lesson,
    Assignment,
    Assessment,
    Problem
}

public class CourseResource : BaseEntity, ISoftDeletable
{
    public long ModuleId { get; set; }

    public ResourceType Type { get; set; }
    public string Title { get; set; } = default!;
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
    public ICollection<ResourceProgress> Progress { get; set; } = [];
}
