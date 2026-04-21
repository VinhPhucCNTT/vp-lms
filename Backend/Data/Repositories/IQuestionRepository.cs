using Backend.Models.Assessments;

namespace Backend.Data.Repositories;

public interface IQuestionRepository
{
    Task<AssessmentQuestion?> GetQuestionByIdAsync(Guid id);

    void Add(AssessmentQuestion question);

    void Update(AssessmentQuestion question);

    void Remove(AssessmentQuestion question);
}

