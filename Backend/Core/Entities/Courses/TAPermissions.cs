using Backend.Core.Common.Models;
using Backend.Core.Entities.Users;

namespace Backend.Core.Entities.Courses;

public class TAPermissions : BaseEntity
{
    public long EnrollmentId { get; set; }
    public bool CanGrade { get; set; } = true;
    // // TODO: Decide whether to keep this
    // public bool CanModerateDiscussions { get; set; } = true;
    // public bool CanEditContent { get; set; } = false;
    // public bool CanManageEnrollments { get; set; } = false;
    public long GrantedByUserId { get; set; }
    public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Enrollment Enrollment { get; set; } = default!;
    public User GrantedByUser { get; set; } = default!;
}
