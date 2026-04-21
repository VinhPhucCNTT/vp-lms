namespace Backend.Features.Assignments.Dtos;

public record CreateAssignmentDto(
    string Instructions,
    DateTime? DueDate,
    bool AllowLateSubmission,
    decimal MaxPoints
);
