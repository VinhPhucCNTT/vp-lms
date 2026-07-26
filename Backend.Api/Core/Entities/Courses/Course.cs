using Backend.Api.Core.Entities.Users;
using Backend.Api.Core.Common.Models;

namespace Backend.Api.Core.Entities.Courses;

public class Course : BaseEntity, ISoftDeletable
{
    public long CreatorId { get; set; }
    public string Code { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsPublished { get; set; } = false;
    public bool EnrollmentOpen { get; set; } = true;

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public User Creator { get; set; } = default!;
    public ICollection<CourseModule> Modules { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
}
