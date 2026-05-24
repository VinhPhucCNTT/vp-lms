using Backend.Common.Models;
using Backend.Models.Users;

namespace Backend.Models.Courses;

public class Enrollment : BaseEntity, ISoftDeletable
{
    public Guid CourseId { get; set; }
    public Guid UserId { get; set; }
    public EnrollmentRole Role { get; set; } = EnrollmentRole.Student;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Course Course { get; set; } = default!;
    public User User { get; set; } = default!;
    public TAPermissions? TAPermissions { get; set; }
}

public enum EnrollmentRole
{
    Student,
    TA,
    Creator
}
