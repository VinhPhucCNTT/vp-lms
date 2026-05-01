using Backend.Models.Common;
using Backend.Models.Courses;

namespace Backend.Models.Resources;

public class Lesson : BaseEntity
{
    public Guid ResourceId { get; set; }
    public string ContentMarkdown { get; set; } = default!;

    // Navigation property
    public ModuleResource Resource { get; set; } = default!;
}
