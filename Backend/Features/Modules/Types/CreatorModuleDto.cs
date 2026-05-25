namespace Backend.Features.Modules.Types;

public record CModuleResponse(
    string Title,
    string? Description,
    int OrderIndex,
    bool IsPublished
);

public record CModuleRequest(
    int PageNumber,
    int PageSize,
    string? Title,
    string? Description,
    bool? IsPublished
);

public record CModuleSetRequest(
    string Title,
    string? Description,
    int OrderIndex,
    bool IsPublished
);

public record CModuleSetResponse(
    string Title,
    string? Description,
    int OrderIndex,
    bool IsPublished
);
