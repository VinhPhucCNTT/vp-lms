namespace Backend.Features.Assignments.Dtos;

public record ViewAssignmentDto(
    Guid Id,
    string Instructions,
    DateTime? DueDate,
    bool AllowLateSubmission,
    decimal MaxPoints
);
