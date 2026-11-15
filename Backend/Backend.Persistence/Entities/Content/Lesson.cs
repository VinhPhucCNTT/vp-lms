using Backend.Persistence.Common;
using Backend.Persistence.Entities.Courses;

namespace Backend.Persistence.Entities.Content;

public class Lesson : BaseEntity, ISoftDeletable
{
    public long ActivityId { get; set; }
    public required string ContentMarkdown { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation property
    public CourseActivity Activity { get; set; } = default!;
}
