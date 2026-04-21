using Backend.Models.Courses;
using Backend.Models.Enums;

namespace Backend.Features.Activities;

internal static class CourseActivityExtensions
{
    internal static Guid? GetResourceId(this CourseActivity a) =>
        a.Type switch
        {
            ActivityType.Lesson => a.Lesson?.Id,
            ActivityType.Assessment => a.Assessment?.Id,
            ActivityType.Assignment => a.Assignment?.Id,
            _ => null
        };
}
