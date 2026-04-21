using Backend.Features.Activities.Dtos;

namespace Backend.Features.Modules.Dtos;

public record ViewModuleDto(
    Guid Id,
    string Title,
    int OrderIndex,
    List<ViewActivityDto>? Activities
);
