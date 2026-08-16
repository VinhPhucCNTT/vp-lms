using Backend.Persistence.Common;

namespace Backend.Persistence.Entities.Users;

public class StudentInfo : BaseEntity
{
    public long UserId { get; set; }

    // Navigation properties
    public User User { get; set; } = default!;
}
