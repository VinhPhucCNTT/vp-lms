using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Courses;

namespace Backend.Api.Core.Entities.Content;

public class Lesson : BaseEntity, ISoftDeletable
{
    public long ResourceId { get; set; }
    public string ContentMarkdown { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation property
    public CourseResource Resource { get; set; } = default!;
}
