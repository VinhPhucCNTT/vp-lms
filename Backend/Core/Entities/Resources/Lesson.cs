using Backend.Core.Common.Models;
using Backend.Core.Entities.Courses;

namespace Backend.Core.Entities.Resources;

public class Lesson : BaseEntity, ISoftDeletable
{
    public long ResourceId { get; set; }
    public string ContentMarkdown { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation property
    public CourseResource Resource { get; set; } = default!;
}
