namespace Backend.Services.Modules.Types;

public enum CModuleGetError
{
    Unauthorized = 0,
    NotFound = 1
}

public enum CModuleSetError
{
    Unauthorized = 0,
    InvalidRequest = 1,
    InternalError = 2
}

public enum CModuleDeleteStatus
{
    Success = 0,
    Unauthorized = 1,
    InvalidRequest = 2,
    InternalError = 3
}

