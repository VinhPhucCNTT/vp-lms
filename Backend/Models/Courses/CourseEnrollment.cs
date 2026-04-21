using Backend.Models.Enums;
using Backend.Models.Users;

namespace Backend.Models.Courses;

public class CourseEnrollment
{
    public Guid UserId { get; set; }

    public ApplicationUser User { get; set; } = null!;

    public Guid CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public EnrollmentStatus Status { get; set; }

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
}
