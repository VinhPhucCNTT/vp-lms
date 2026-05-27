namespace Backend.Services.Courses.Types;

public enum CCourseGetError
{
    Unauthorized = 0,
    NotFound = 1
}

public enum CCourseQueryError
{
    Unauthorized = 0,
    InvalidRequest = 1,
    InternalError = 2
}

public enum CCourseSetError
{
    Unauthorized = 0,
    InvalidRequest = 1,
    InternalError = 2
}

public enum CCourseDeleteStatus
{
    Success = 0,
    Unauthorized = 1,
    InvalidRequest = 2,
    InternalError = 3
}
