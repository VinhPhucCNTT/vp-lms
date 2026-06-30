using System.ComponentModel.DataAnnotations;
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

public class ModuleResource : BaseEntity, ISoftDeletable, IValidatableObject
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

    public long? LessonId { get; set; }
    public long? AssignmentId { get; set; }
    public long? AssessmentId { get; set; }
    public long? ProblemId { get; set; }

    public Lesson? Lesson { get; set; }
    public Assignment? Assignment { get; set; }
    public Assessment? Assessment { get; set; }
    public CodingProblem? Problem { get; set; }

    // Navigation properties
    public CourseModule Module { get; set; } = default!;
    public ICollection<ResourceComment> Comments { get; set; } = [];
    public ICollection<ResourceProgress> Progress { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        int count = 0;
        if (LessonId.HasValue) count++;
        if (AssignmentId.HasValue) count++;
        if (AssessmentId.HasValue) count++;
        if (ProblemId.HasValue) count++;

        if (count == 0)
            yield return new ValidationResult("Module resource has not been initialized.", [ nameof(LessonId), nameof(AssignmentId), nameof(AssessmentId), nameof(ProblemId) ]);
        if (count > 1)
            yield return new ValidationResult("Module resource may only belong to one resource type.", [ nameof(LessonId), nameof(AssignmentId), nameof(AssessmentId), nameof(ProblemId) ]);
    }
}
