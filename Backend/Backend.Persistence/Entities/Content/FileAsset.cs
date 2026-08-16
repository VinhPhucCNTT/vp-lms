using Backend.Persistence.Common;
using Backend.Persistence.Entities.Assignments;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Users;

namespace Backend.Persistence.Entities.Content;

public class FileAsset : BaseEntity, ISoftDeletable
{
    public long UserId { get; set; }

    public string OriginalFileName { get; set; } = null!;
    public string StoragePath { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long SizeInBytes { get; set; }
    public string? Sha256Hash { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public User User { get; set; } = default!;
    public Course? Course { get; set; } // Course background
    public ICollection<AssignmentFile> AssignmentFiles { get; set; } = [];
}
