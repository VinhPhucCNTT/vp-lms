using Backend.Models.Courses;
using Backend.Models.Common;

namespace Backend.Models.Lessons;

public class Lesson : BaseEntity
{
    public Guid ActivityId { get; set; }

    public CourseActivity Activity { get; set; } = null!;

    public string ContentHtml { get; set; } = null!;

    public string? VideoUrl { get; set; }

    public string? AttachmentUrl { get; set; }
}
