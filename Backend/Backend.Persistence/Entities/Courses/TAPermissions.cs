using Backend.Persistence.Common;
using Backend.Persistence.Entities.Users;

namespace Backend.Persistence.Entities.Courses;

public class TAPermissions : BaseEntity
{
    public long EnrollmentId { get; set; }
    public bool CanGrade { get; set; } = true;
    public long GrantedByUserId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Enrollment Enrollment { get; set; } = default!;
    public User GrantedByUser { get; set; } = default!;
}
