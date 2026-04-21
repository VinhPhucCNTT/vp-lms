using Backend.Models.Enums;

namespace Backend.Features.Activities.Dtos;

public record CreateActivityDto(
    string Title,
    ActivityType Type,
    int OrderIndex,
    bool IsPublished,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil
);
