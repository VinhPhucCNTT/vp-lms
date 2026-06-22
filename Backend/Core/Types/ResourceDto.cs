using Backend.Core.Entities.Courses;

namespace Backend.Core.Types;

public record ResourceResponse(
    string Id,
    ResourceType Type,
    string Title,
    int OrderIndex
) : IEntityResponse;

public record ResourceDetailResponse(
    string Id,
    ResourceType Type,
    string Title,
    string? Description,
    int OrderIndex,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    DateTime CreatedAt,
    DateTime UpdatedAt
) : IEntityResponse;

// public record ResourceRequest();

public record ResourceCreateRequest(
    ResourceType Type,
    string Title,
    string? Description,
    int OrderIndex,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    bool IsPublished,
    string? AccessPassword
);

public record ResourceUpdateRequest(
    string Title,
    string? Description,
    int OrderIndex,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    bool IsPublished,
    string? AccessPassword
);

public record ResourceSetResponse(
    string Id,
    ResourceType Type,
    string Title,
    string? Description,
    int OrderIndex,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    bool IsPublished,
    string? AccessPassword
) : IEntityResponse;
