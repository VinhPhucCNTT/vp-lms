using System.Text.Json;
using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Users;

namespace Backend.Api.Core.Entities.Assessments;

public class AttemptAnswer : BaseEntity
{
    public long? GraderId { get; set; }
    public long AttemptQuestionId { get; set; }

    required public JsonDocument AnswerData { get; set; }
    public decimal? EarnedPoints { get; set; }
    public bool? IsCorrect { get; set; }
    public DateTime? AnsweredAt { get; set; }
    public DateTime? GradedAt { get; set; }

    // Navigation properties
    public AttemptQuestion AttemptQuestion { get; set; } = default!;
    public User? Grader { get; set; }
}
