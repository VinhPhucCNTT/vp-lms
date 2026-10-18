namespace Backend.Persistence.Common;

public class LanguageProfileStruct
{
    public long CompilerId { get; set; }

    public string Name { get; set; } = "";

    public string CompilerCmd { get; set; } = "";

    public string RunCmd { get; set; } = "";
}
