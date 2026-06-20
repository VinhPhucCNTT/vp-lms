using Backend.Core.Common.Models;
using Backend.Core.Entities.Resources;

namespace Backend.Core.Entities.Courses;

public enum ResourceType
{
    Lesson,
    Assignment,
    Assessment,
    Problem
}

public class ModuleResource : BaseEntity, ISoftDeletable
{
    public long ModuleId { get; set; }
    public ResourceType ResourceType { get; set; }
    public string Title { get; set; } = default!;
    // TODO: This might be redundant...
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsPublished { get; set; } = false;
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableUntil { get; set; }
    public string? AccessPassword { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public CourseModule Module { get; set; } = default!;
    public Lesson? Lesson { get; set; }
    public Assignment? Assignment { get; set; }
    public Assessment? Assessment { get; set; }
    public CodingProblem? Problem { get; set; }
    public ICollection<ResourceComment> Comments { get; set; } = [];
    public ICollection<ResourceProgress> Progress { get; set; } = [];
}
