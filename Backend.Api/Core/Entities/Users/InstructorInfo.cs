using Backend.Api.Core.Common.Models;

namespace Backend.Api.Core.Entities.Users;

public class InstructorInfo : BaseEntity
{
    public long UserId { get; set; }

    // Navigation properties
    public User User { get; set; } = default!;
}
