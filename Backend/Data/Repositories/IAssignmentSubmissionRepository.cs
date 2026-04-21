using Backend.Models.Assignments;

namespace Backend.Data.Repositories;

public interface IAssignmentSubmissionRepository
{
    Task<AssignmentSubmission?> GetByIdAsync(Guid id);

    Task<List<AssignmentSubmission>> GetByAssignmentIdAsync(Guid assignmentId);

    void Add(AssignmentSubmission submission);

    void Update(AssignmentSubmission submission);
}
