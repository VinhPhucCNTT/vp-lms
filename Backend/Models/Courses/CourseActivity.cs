using Backend.Models.Assessments;
using Backend.Models.Assignments;
using Backend.Models.Enums;
using Backend.Models.Lessons;
using Backend.Models.Common;

namespace Backend.Models.Courses;

public class CourseActivity : BaseEntity
{
    public Guid ModuleId { get; set; }

    public CourseModule Module { get; set; } = null!;

    public string Title { get; set; } = null!;

    public ActivityType Type { get; set; }

    public int OrderIndex { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? AvailableFrom { get; set; }

    public DateTime? AvailableUntil { get; set; }

    public Lesson? Lesson { get; set; }

    public Assessment? Assessment { get; set; }

    public Assignment? Assignment { get; set; }
}
