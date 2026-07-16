using Backend.Core.Common.Models;
using Backend.Core.Entities.Users;

namespace Backend.Core.Entities.Courses;

public class CourseAnnouncement : BaseEntity
{
    public long CourseId { get; set; }
    public long UserId { get; set; }
    public string ContentMarkdown { get; set; } = default!;

    public Course Course { get; set; } = default!;
    public User User { get; set; } = default!;
}
