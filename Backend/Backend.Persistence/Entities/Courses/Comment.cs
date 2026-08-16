using Backend.Persistence.Common;
using Backend.Persistence.Entities.Users;

namespace Backend.Persistence.Entities.Courses;

public class Comment : BaseEntity
{
    public long ActivityId { get; set; }
    public long UserId { get; set; }
    public long? ParentCommentId { get; set; }
    public string ContentMarkdown { get; set; } = default!;
    public bool IsEdited { get; set; } = false;

    // Soft delete by anonymization
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public CourseResource Resource { get; set; } = default!;
    public User User { get; set; } = default!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = [];
}
