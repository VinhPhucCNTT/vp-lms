using Backend.Models.Users;
using Backend.Models.Common;

namespace Backend.Models.Courses;

public class Course : BaseEntity, ISoftDeletable
{
    public Guid CreatorId { get; set; }
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string Slug { get; set; } = default!;
    public string? ThumbnailUrl { get; set; }
    public bool IsPublished { get; set; } = false;
    public bool AllowAnonymousAccess { get; set; } = false;
    public bool EnrollmentOpen { get; set; } = true;

    // Soft delete
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public User Creator { get; set; } = default!;
    public ICollection<Module> Modules { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}
