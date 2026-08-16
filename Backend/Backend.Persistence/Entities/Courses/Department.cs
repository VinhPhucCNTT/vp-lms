using Backend.Persistence.Common;

namespace Backend.Persistence.Entities.Courses;

public class Department : BaseEntity
{
    public string Name { get; set; } = "";

    // Navigation properties
    public ICollection<Course> Courses = [];
}
