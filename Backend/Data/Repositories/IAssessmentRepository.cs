using Backend.Models.Assessments;

namespace Backend.Data.Repositories;

public interface IAssessmentRepository
{
    Task<Assessment?> GetByIdAsync(Guid id);

    Task<Assessment?> GetFullContentByIdAsync(Guid id);

    Task<Assessment?> GetDeletedByIdAsync(Guid id);

    void Add(Assessment assessment);

    void Update(Assessment assessment);

    void Remove(Assessment assessment);

    void Restore(Assessment assessment);

    void HardDelete(Assessment assessment);
}
