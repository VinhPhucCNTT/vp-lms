using Backend.Common.Models;
using Backend.Models.Users;

namespace Backend.Models.Courses;

public class ResourceProgress : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid ResourceId { get; set; }
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User User { get; set; } = default!;
    public ModuleResource Resource { get; set; } = default!;
}
