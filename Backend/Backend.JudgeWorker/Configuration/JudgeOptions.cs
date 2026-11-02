namespace Backend.JudgeWorker.Configuration;

public class JudgeOptions
{
    required public string WorkspaceRelativeDir { get; set; }

    required public int CpuLimit { get; set; }

    required public int PidsLimit { get; set; }
}
