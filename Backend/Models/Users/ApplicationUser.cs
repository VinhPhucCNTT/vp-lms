using Backend.Models.Assessments;
using Backend.Models.Assignments;
using Backend.Models.Courses;
using Microsoft.AspNetCore.Identity;

namespace Backend.Models.Users;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FullName { get; set; } = default!;
    public string? AvatarUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Course> CreatedCourses { get; set; } = [];
    public ICollection<CourseEnrollment> CourseEnrollments { get; set; } = [];
    public ICollection<AssessmentAttempt> AssessmentAttempts { get; set; } = [];
    public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = [];
}
