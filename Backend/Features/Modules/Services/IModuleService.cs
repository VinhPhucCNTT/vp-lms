using Backend.Features.Modules.Dtos;

namespace Backend.Features.Modules.Services;

public interface IModuleService
{
    Task<Guid> CreateAsync(Guid courseId, CreateModuleDto dto);

    Task<List<ViewModuleDto>> GetByCourseAsync(Guid courseId);

    Task<bool> UpdateAsync(Guid id, UpdateModuleDto dto);

    Task<bool> DeleteAsync(Guid id);
}
