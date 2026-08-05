using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Assignments;
using Backend.Api.Core.Entities.Courses;
using Backend.Api.Core.Entities.Users;

namespace Backend.Api.Core.Entities.Content;

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
