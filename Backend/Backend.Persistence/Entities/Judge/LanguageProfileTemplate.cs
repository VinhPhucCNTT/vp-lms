using Backend.Persistence.Common;
using Backend.Persistence.Entities.Users;

namespace Backend.Persistence.Entities.Judge;

// NOTE: Not needed for now
public class LanguageProfileTemplate : BaseEntity
{
    public long UserId { get; set; }

    public string Name { get; set; } = default!;
    // public LanguageProfileStruct[] LanguageProfile { get; set; } = default!;

    // Navigation properties
    public User User { get; set; } = default!;
}
