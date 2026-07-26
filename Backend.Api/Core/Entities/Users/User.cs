using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Content;
using Backend.Api.Core.Entities.Courses;

namespace Backend.Api.Core.Entities.Users;

public class User : BaseEntity
{
    public string Username { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string Fullname { get; set; } = default!;
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public UserRoles Role { get; set; }

    // Soft delete using anonymized name
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<Course> Courses { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<FileAsset> FileAssets { get; set; } = [];
}

public enum UserRoles {
    Student,
    Instructor,
    Admin
}
