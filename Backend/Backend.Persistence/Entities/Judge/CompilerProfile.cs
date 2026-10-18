using Backend.Persistence.Common;
using Backend.Persistence.Entities.Content;

namespace Backend.Persistence.Entities.Judge;

public class CompilerProfile : BaseEntity
{
    public long ProblemId { get; set; }
    public long CompilerId { get; set; }

    public string Name { get; set; } = "";
    public string CompileCmd { get; set; } = "";
    public string RunCmd { get; set; } = "";

    // Navigation properties
    public CodingProblem Problem { get; set; } = default!;
    public Compiler Compiler { get; set; } = default!;
}
