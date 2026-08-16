using Backend.Persistence.Entities.Users;
using Backend.Persistence.Common;
using Backend.Persistence.Entities.Content;

namespace Backend.Persistence.Entities.Courses;

public class Course : BaseEntity, ISoftDeletable
{
    public long CreatorId { get; set; }
    public long? DepartmentId { get; set; }

    public string Code { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string? Description { get; set; }
    public bool IsPublished { get; set; } = false;
    public bool EnrollmentOpen { get; set; } = true;
    public long? BackgroundFileId { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public Department? Department { get; set; }
    public User Creator { get; set; } = default!;
    public ICollection<CourseModule> Modules { get; set; } = [];
    public ICollection<Enrollment> Enrollments { get; set; } = [];
    public FileAsset? BackgroundFile { get; set; }
}
