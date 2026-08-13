using System.Text.Json;
using Backend.Api.Core.Common.Models;
using Backend.Api.Core.Entities.Content;

namespace Backend.Api.Core.Entities.Assessments;

public class AssessmentQuestion : BaseEntity
{
    public long AssessmentId { get; set; }
    public long QuestionId { get; set; }

    public int OrderIndex { get; set; }
    public decimal Points { get; set; } = 1;

    // Navigation properties
    public Assessment Assessment { get; set; } = default!;
    public ICollection<AttemptAnswer> Answers { get; set; } = [];
}
