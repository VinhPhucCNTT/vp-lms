using Backend.Persistence.Entities.Users;

namespace Backend.Persistence.Entities.Courses;

public class RecentCourse
{
    public long UserId { get; set; }
    public long CourseId { get; set; }

    public DateTime LastAccessedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = default!;
    public Course Course { get; set; } = default!;
}

