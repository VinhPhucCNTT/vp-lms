namespace Backend.Features.Assignments.Dtos;

public record UpdateAssignmentDto(
    string Instructions,
    DateTime? DueDate,
    bool AllowLateSubmission,
    decimal MaxPoints
);
