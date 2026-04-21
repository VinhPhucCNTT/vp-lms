using Backend.Models.Common;

namespace Backend.Models.Courses;

public class CourseModule : BaseEntity
{
    public Guid CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string Title { get; set; } = null!;

    public int OrderIndex { get; set; }

    public ICollection<CourseActivity> Activities { get; set; } = [];
}
