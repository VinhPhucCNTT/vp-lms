using Backend.Persistence.Common;
using Backend.Persistence.Entities.Users;

namespace Backend.Persistence.Entities.Courses;

public class Comment : BaseEntity
{
    public long ActivityId { get; set; }
    public long UserId { get; set; }
    public long? ParentCommentId { get; set; }
    public required string ContentMarkdown { get; set; }
    public bool IsEdited { get; set; } = false;

    // Soft delete by anonymization
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }

    // Navigation properties
    public CourseActivity Activity { get; set; } = default!;
    public User User { get; set; } = default!;
    public Comment? ParentComment { get; set; }
    public ICollection<Comment> Replies { get; set; } = [];
}
