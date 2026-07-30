using Backend.Api.Core.Common.Models;

namespace Backend.Api.Core.Entities.Courses;

public class Department : BaseEntity
{
    public string Name { get; set; } = "";

    // Navigation properties
    public ICollection<Course> Courses = [];
}
