using Backend.Core.Common.Models;

namespace Backend.Core.Entities.Courses;

public class Department : BaseEntity
{
    public string Name { get; set; } = default!;

    // Navigation properties
    public ICollection<Course> Courses { get; set; } = [];
}
