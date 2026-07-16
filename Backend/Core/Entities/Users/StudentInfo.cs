namespace Backend.Core.Entities.Users;

public class StudentInfo : BaseEntity
{
    public long UserId { get; set; }

    // Navigation properties
    public User User { get; set; } = default!;
}
