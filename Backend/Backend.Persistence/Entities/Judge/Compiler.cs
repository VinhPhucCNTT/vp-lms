using Backend.Persistence.Common;

namespace Backend.Persistence.Entities.Judge;

public class Compiler : BaseEntity
{
    public string Name { get; set; } = "";

    public string ApplicationName { get; set; } = "";

    public string Version { get; set; } = "";
}
