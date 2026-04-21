using Backend.Models.Enums;

namespace Backend.Features.Activities.Dtos;

public record ViewActivityDto(
    Guid Id,
    string Title,
    ActivityType Type,
    int OrderIndex,
    bool IsPublished,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    /// <summary>Lesson, Assignment, or Assessment entity id for detail APIs (distinct from activity id).</summary>
    Guid? ResourceId
);
