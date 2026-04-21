using Backend.Data.Repositories;
using Backend.Data.UnitOfWork;
using Backend.Features.Assignments.Dtos;
using Backend.Models.Assignments;

namespace Backend.Features.Assignments.Services;

public class AssignmentService(
    IAssignmentRepository repo,
    IAssignmentSubmissionRepository submissionRepo,
    IUnitOfWork uow) : IAssignmentService
{
    private readonly IAssignmentRepository _repo = repo;
    private readonly IAssignmentSubmissionRepository _submissionRepo = submissionRepo;
    private readonly IUnitOfWork _uow = uow;

    public async Task<Guid> CreateAsync(
        Guid activityId,
        CreateAssignmentDto dto)
    {
        var assignment = new Assignment
        {
            ActivityId = activityId,
            Instructions = dto.Instructions,
            DueDate = dto.DueDate,
            AllowLateSubmission = dto.AllowLateSubmission,
            MaxPoints = dto.MaxPoints
        };

        _repo.Add(assignment);

        await _uow.SaveChangesAsync();

        return assignment.Id;
    }

    public async Task<ViewAssignmentDto?> GetByIdAsync(Guid id)
    {
        var assignment = await _repo.GetByIdAsync(id);
        if (assignment is null)
            return null;

        return new ViewAssignmentDto(
            assignment.Id,
            assignment.Instructions,
            assignment.DueDate,
            assignment.AllowLateSubmission,
            assignment.MaxPoints
        );
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateAssignmentDto dto)
    {
        var assignment = await _repo.GetByIdAsync(id);
        if (assignment is null)
            return false;

        assignment.Instructions = dto.Instructions;
        assignment.DueDate = dto.DueDate;
        assignment.AllowLateSubmission = dto.AllowLateSubmission;
        assignment.MaxPoints = dto.MaxPoints;

        _repo.Update(assignment);

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var assignment = await _repo.GetByIdAsync(id);
        if (assignment is null)
            return false;

        _repo.Remove(assignment);

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task SubmitAsync(
        Guid assignmentId,
        Guid studentId,
        SubmitAssignmentDto dto)
    {
        var submission = new AssignmentSubmission
        {
            AssignmentId = assignmentId,
            StudentId = studentId,
            SubmissionText = dto.SubmissionText,
            SubmittedAt = DateTime.UtcNow
        };

        _submissionRepo.Add(submission);

        await _uow.SaveChangesAsync();
    }
}
