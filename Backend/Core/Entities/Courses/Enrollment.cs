using Backend.Core.Common.Models;
using Backend.Core.Entities.Users;

namespace Backend.Core.Entities.Courses;

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
    TA
}
