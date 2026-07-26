using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Courses;
using Backend.Api.Core.Entities.Users;

namespace Backend.Api.Core.Entities.Learning;

public class ResourceProgress : BaseEntity
{
    public long UserId { get; set; }
    public long ResourceId { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = default!;
    public CourseResource Resource { get; set; } = default!;
}
