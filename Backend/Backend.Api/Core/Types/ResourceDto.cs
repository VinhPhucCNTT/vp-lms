using Backend.Persistence.Entities.Courses;

namespace Backend.Api.Core.Types;

public record ResourceResponse(
    string Id,
    ResourceType Type,
    string Title,
    int OrderIndex
) : IEntityResponse;

public record ResourceListResponse(
    string Id,
    ResourceType Type,
    CourseStudentResponse CourseInfo,
    DateOnly DueDate,
    bool StatusIsCompleted // false = Pending
) : IEntityResponse;

public record ResourceDetailResponse(
    string Id,
    ResourceType Type,
    string Title,
    int OrderIndex,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IEntityResponse;

public record ResourceRequestInfo
(
    string Title,
    int OrderIndex,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    bool IsPublished,
    string? AccessPassword
);
