using Backend.Models.Users;
using Backend.Models.Common;

namespace Backend.Models.Courses;

public class Course : BaseEntity, ISoftDeletable
{
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public string? ThumbnailUrl { get; set; }

    public bool IsPublished { get; set; }

    public Guid InstructorId { get; set; }

    public ApplicationUser Instructor { get; set; } = null!;

    public ICollection<CourseModule> Modules { get; set; } = [];

    public ICollection<CourseEnrollment> Enrollments { get; set; } = [];

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }
}
