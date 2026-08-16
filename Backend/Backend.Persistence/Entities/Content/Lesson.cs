using Backend.Persistence.Common;
using Backend.Persistence.Entities.Courses;

namespace Backend.Persistence.Entities.Content;

public class Lesson : BaseEntity, ISoftDeletable
{
    public long ResourceId { get; set; }
    public string ContentMarkdown { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation property
    public CourseResource Resource { get; set; } = default!;
}
