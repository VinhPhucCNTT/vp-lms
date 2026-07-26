using System.ComponentModel.DataAnnotations;
using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Content;

namespace Backend.Api.Core.Entities.Courses;

public enum ResourceType
{
    Lesson,
    Assignment,
    Assessment,
    Problem
}

public class CourseResource : BaseEntity, ISoftDeletable, IValidatableObject
{
    public long ModuleId { get; set; }
    public ResourceType Type { get; set; }
    public string Title { get; set; } = default!;
    public int OrderIndex { get; set; }
    public bool IsPublished { get; set; } = false;
    public DateTime? AvailableFrom { get; set; }
    public DateTime? AvailableUntil { get; set; }
    public string? AccessPassword { get; set; }

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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        int count = 0;
        if (Lesson != null) count++;
        if (Assignment != null) count++;
        if (Assessment != null) count++;
        if (Problem != null) count++;

        if (count == 0)
            yield return new ValidationResult("Module resource has not been initialized.", [ nameof(Lesson), nameof(Assignment), nameof(Assessment), nameof(Problem) ]);
        if (count > 1)
            yield return new ValidationResult("Module resource may only belong to one resource type.", [ nameof(Lesson), nameof(Assignment), nameof(Assessment), nameof(Problem) ]);
    }
}
