using Backend.Common.Models;
using Backend.Models.Users;

namespace Backend.Models.Courses;

public class TAPermissions : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public bool CanGrade { get; set; } = true;
    public bool CanModerateDiscussions { get; set; } = true;
    public bool CanEditContent { get; set; } = false;
    public bool CanManageEnrollments { get; set; } = false;
    public Guid GrantedByUserId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Enrollment Enrollment { get; set; } = default!;
    public User GrantedByUser { get; set; } = default!;
}
