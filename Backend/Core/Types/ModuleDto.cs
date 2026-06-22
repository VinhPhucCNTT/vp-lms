namespace Backend.Core.Types;

public record ModuleResponse(
    string Id,
    string Title,
    int OrderIndex
) : IEntityResponse;

public record ModuleDetailResponse(
    string Id,
    string Title,
    string? Description,
    int OrderIndex
) : IEntityResponse;

// public record ModuleRequest();

public record ModuleSetRequest(
    string Title,
    string? Description,
    int OrderIndex,
    bool IsPublished
);

public record ModuleSetResponse(
    string Id,
    string Title,
    string? Description,
    int OrderIndex,
    bool? IsPublished
) : IEntityResponse;
