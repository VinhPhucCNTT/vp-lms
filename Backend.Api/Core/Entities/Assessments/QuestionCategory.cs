using Backend.Api.Core.Common.Models;

namespace Backend.Api.Core.Entities.Assessments;

public class QuestionCategory : BaseEntity
{
    public long QuestionBankId { get; set; }

    public string Name { get; set; } = default!;

    // Navigation properties
    public ICollection<Question> Questions { get; set; } = [];
    public QuestionBank QuestionBank { get; set; } = default!;
}
