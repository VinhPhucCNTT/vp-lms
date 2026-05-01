using Backend.Models.Common;
using Backend.Models.Courses;

namespace Backend.Models.Users;

public class User : BaseEntity, ISoftDeletable
{
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<Course> Courses { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}
