namespace Backend.Features.Courses.Types;

public enum CourseDeleteStatus
{
    Success = 0,
    InvalidUser = 1,
    InvalidCourse = 2,
    InternalError = 3
}

public enum CourseSetStatus
{
    Success = 0,
    InvalidUser = 1,
    InvalidCourse = 2,
    InternalError = 3
}

public class CourseSetResult
{
    public CourseSetResponse? Response { get; set; }

    public CourseSetStatus Status { get; set; }
}
