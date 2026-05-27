using Backend.Core.Common.Models;
using Backend.Core.Entities.Courses;

namespace Backend.Core.Entities.Resources;

public class Lesson : BaseEntity, ISoftDeletable
{
    public Guid ResourceId { get; set; }
    public string ContentMarkdown { get; set; } = default!;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation property
    public ModuleResource Resource { get; set; } = default!;
}
