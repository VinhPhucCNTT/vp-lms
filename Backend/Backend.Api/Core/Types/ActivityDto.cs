using Backend.Persistence.Entities.Courses;

namespace Backend.Api.Core.Types;

public record ActivityResponse(
    string Id,
    ActivityType Type,
    string Title,
    int OrderIndex
) : IEntityResponse;

public record StudentActivityResponse(
    string Id,
    ActivityType Type,
    string Title,
    int OrderIndex,
    bool StatusIsCompleted
) : IEntityResponse;

public record InstructorActivityResponse(
    string Id,
    ActivityType Type,
    string Title,
    int OrderIndex,
    bool IsPublished
) : IEntityResponse;

public record ActivityDetailResponse(
    string Id,
    ActivityType Type,
    string Title,
    int OrderIndex,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IEntityResponse;

public record ActivityProgressResponse(
    bool IsCompleted,
    DateTime? CompletedAt,
    DateTime? LastAccessedAt
);

public record ActivityRequestInfo
(
    string Title,
    int OrderIndex,
    bool IsPublished
);
