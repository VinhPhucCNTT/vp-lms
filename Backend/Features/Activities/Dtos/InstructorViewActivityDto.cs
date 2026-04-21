using Backend.Models.Enums;

namespace Backend.Features.Activities.Dtos;

public record InstructorViewActivityDto(
    Guid Id,
    string Title,
    ActivityType Type,
    int OrderIndex,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil
);
