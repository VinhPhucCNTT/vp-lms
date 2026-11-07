using Backend.Persistence.Common;
using Backend.Persistence.Entities.Assessments;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Courses;

namespace Backend.Persistence.Entities.Users;

public class User : BaseEntity
{
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public long? AvatarFileId { get; set; }
    public bool IsActive { get; set; } = true;
    public required UserRoles Role { get; set; }

    // Soft delete using anonymized name
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ICollection<Course> Courses { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
    public ICollection<FileAsset> FileAssets { get; set; } = [];
    public ICollection<QuestionBank> QuestionBanks { get; set; } = [];
    public ICollection<RecentCourse> RecentCourses { get; set; } = [];
}

public enum UserRoles {
    Student,
    Instructor,
    Admin
}
