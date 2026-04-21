using Backend.Models.Assignments;

namespace Backend.Data.Repositories;

public interface IAssignmentRepository
{
    Task<Assignment?> GetByIdAsync(Guid id);

    Task<Assignment?> GetDeletedByIdAsync(Guid id);

    void Add(Assignment assignment);

    void Update(Assignment assignment);

    void Remove(Assignment assignment);

    void Restore(Assignment assignment);

    void HardDelete(Assignment assignment);
}
