using Backend.Features.Modules.Dtos;

namespace Backend.Features.Courses.Dtos;

public record CourseContentDto(
    Guid Id,
    string Title,
    List<ViewModuleDto> Modules
);
