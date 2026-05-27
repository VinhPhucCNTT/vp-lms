using Backend.Core.Common.Models;
using Backend.Core.Entities.Users;

namespace Backend.Core.Entities.Courses;

public class ResourceComment : BaseEntity
{
    public Guid ResourceId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string ContentMarkdown { get; set; } = default!;
    public bool IsEdited { get; set; } = false;

    // Soft delete by anonymization
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ModuleResource Resource { get; set; } = default!;
    public User User { get; set; } = default!;
    public ResourceComment? ParentComment { get; set; }
    public ICollection<ResourceComment> Replies { get; set; } = [];
}
