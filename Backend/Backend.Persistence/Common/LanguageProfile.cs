namespace Backend.Persistence.Common;

public class LanguageProfile
{
    public long CompilerId { get; set; }

    public string Name { get; set; } = "";

    public string CompilerCmd { get; set; } = "";

    public string RunCmd { get; set; } = "";
}
