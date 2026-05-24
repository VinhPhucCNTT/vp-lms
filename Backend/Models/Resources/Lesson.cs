using Backend.Common.Models;
using Backend.Models.Courses;

namespace Backend.Models.Resources;

public class Lesson : BaseEntity, ISoftDeletable
{
    public Guid ResourceId { get; set; }
    public string ContentMarkdown { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation property
    public ModuleResource Resource { get; set; } = default!;
}
