namespace Backend.Features.Courses.Types;

public enum CourseDeleteStatus
{
    Success = 0,
    Unauthorized = 1,
    InvalidRequest = 2,
    InternalError = 3
}

public enum CourseSetError
{
    Unauthorized = 0,
    InvalidRequest = 1,
    InternalError = 2
}
