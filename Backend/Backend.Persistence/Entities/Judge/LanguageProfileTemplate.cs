using Backend.Persistence.Common;
using Backend.Persistence.Entities.Users;

namespace Backend.Persistence.Entities.Judge;

public class LanguageProfileTemplate : BaseEntity
{
    public long UserId { get; set; }

    public string Name { get; set; } = default!;
    public LanguageProfile[] LanguageProfile { get; set; } = default!;

    // Navigation properties
    public User User { get; set; } = default!;
}
