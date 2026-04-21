using Backend.Models.Common;
using Backend.Models.Courses;
using Backend.Models.Enums;

namespace Backend.Models.Assessments;

public class Assessment : BaseEntity, ISoftDeletable
{
    public Guid ActivityId { get; set; }

    public CourseActivity Activity { get; set; } = null!;

    public AssessmentType Type { get; set; }

    public int TimeLimitMinutes { get; set; }

    public int MaxAttempts { get; set; }

    public string? Password { get; set; }

    public decimal PassingScore { get; set; }

    public bool ShuffleQuestions { get; set; }

    public ICollection<AssessmentQuestion> Questions { get; set; } = [];

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }
}
