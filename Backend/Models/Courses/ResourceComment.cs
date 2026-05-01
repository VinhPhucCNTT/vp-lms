using Backend.Models.Common;
using Backend.Models.Users;

namespace Backend.Models.Courses;

public class ResourceComment : BaseEntity, ISoftDeletable
{
    public Guid ResourceId { get; set; }
    public Guid UserId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public string ContentMarkdown { get; set; } = default!;
    public bool IsEdited { get; set; } = false;
    
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public ModuleResource Resource { get; set; } = default!;
    public User User { get; set; } = default!;
    public ResourceComment? ParentComment { get; set; }
    public ICollection<ResourceComment> Replies { get; set; } = [];
}
