using Backend.Models.Assessments;

namespace Backend.Data.Repositories;

public interface IAssessmentAttemptRepository
{
    Task<AssessmentAttempt?> GetByIdAsync(Guid id);

    Task<List<AssessmentAttempt>> GetByStudentIdAsync(Guid studentId);

    void AddAsync(AssessmentAttempt attempt);

    void Update(AssessmentAttempt attempt);
}
