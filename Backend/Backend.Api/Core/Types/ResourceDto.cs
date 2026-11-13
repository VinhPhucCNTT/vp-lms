using Backend.Persistence.Entities.Courses;

namespace Backend.Api.Core.Types;

public record ResourceResponse(
    string Id,
    ResourceType Type,
    string Title,
    int OrderIndex
) : IEntityResponse;

public record StudentResourceResponse(
    string Id,
    ResourceType Type,
    string Title,
    int OrderIndex,
    bool StatusIsCompleted
) : IEntityResponse;

public record InstructorResourceResponse(
    string Id,
    ResourceType Type,
    string Title,
    int OrderIndex,
    bool IsPublished
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

public record ResourceProgressResponse(
    bool IsCompleted,
    DateTime? CompletedAt,
    DateTime? LastAccessedAt
);

public record ResourceRequestInfo
(
    string Title,
    int OrderIndex,
    bool IsPublished
);
