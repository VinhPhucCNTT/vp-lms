using Backend.Core.Entities.Courses;

namespace Backend.Core.Types;

public record ResourceResponse(
    ResourceType Type,
    string Title,
    int OrderIndex
);

public record ResourceDetailResponse(
    ResourceType Type,
    string Title,
    string? Description,
    int OrderIndex,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil
);

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
    long Id,
    ResourceType Type,
    string Title,
    string? Description,
    int OrderIndex,
    DateTime? AvailableFrom,
    DateTime? AvailableUntil,
    bool IsPublished,
    string? AccessPassword
);
