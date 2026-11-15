using Backend.Persistence.Common;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Users;

namespace Backend.Persistence.Entities.Learning;

public class ActivityProgress : BaseEntity
{
    public long UserId { get; set; }
    public long ActivityId { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = default!;
    public CourseActivity Activity { get; set; } = default!;
}
