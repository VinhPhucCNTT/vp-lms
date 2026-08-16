namespace Backend.Api.Core.Types;

public record ModuleResponse(
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
