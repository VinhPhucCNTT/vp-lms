namespace Backend.Core.Types;

public record ModuleResponse(
    string Title,
    int OrderIndex
);

public record ModuleDetailResponse(
    string Title,
    string? Description,
    int OrderIndex
);

// public record ModuleRequest();

public record ModuleSetRequest(
    string Title,
    string? Description,
    int OrderIndex,
    bool IsPublished
);

public record ModuleSetResponse(
    string ModuleSqid,
    string Title,
    string? Description,
    int OrderIndex,
    bool? IsPublished
);
