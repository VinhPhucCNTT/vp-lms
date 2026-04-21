using Backend.Features.Assignments.Dtos;

namespace Backend.Features.Assignments.Services;

public interface IAssignmentService
{
    Task<Guid> CreateAsync(Guid activityId, CreateAssignmentDto dto);

    Task<ViewAssignmentDto?> GetByIdAsync(Guid id);

    Task<bool> UpdateAsync(Guid id, UpdateAssignmentDto dto);

    Task<bool> DeleteAsync(Guid id);

    Task SubmitAsync(
        Guid assignmentId,
        Guid studentId,
        SubmitAssignmentDto dto);
}
