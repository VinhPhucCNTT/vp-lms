using Backend.Common.Models;

namespace Backend.Models.Courses;

public class Module : BaseEntity, ISoftDeletable
{
    public Guid CourseId { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPublished { get; set; } = false;
    
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Course Course { get; set; } = default!;
    public ICollection<ModuleResource> Resources { get; set; } = [];
}
