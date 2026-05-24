namespace Backend.Features.Modules.Types;

public enum CModuleDeleteStatus
{
    Success = 0,
    InvalidUser = 1,
    InvalidCourse = 2,
    InvalidModule = 3,
    InternalError = 4
}

public enum ModuleSetStatus
{
    Success = 0,
    InvalidUser = 1,
    InvalidCourse = 2,
    InvalidModule = 3,
    InternalError = 4
}

public class CModuleSetResult
{
    public CModuleSetResponse? Response { get; set; }

    public ModuleSetStatus Status { get; set; }
}
