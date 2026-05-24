namespace Backend.Features.Modules.Types;

public record ModuleResponse(
    string Title,
    string? Description,
    int OrderIndex
);

public record ModuleRequest(
    int PageNumber,
    int PageSize,
    string? Title,
    string? Description
);
