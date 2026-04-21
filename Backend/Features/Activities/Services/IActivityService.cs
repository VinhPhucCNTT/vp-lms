using Backend.Features.Activities.Dtos;

namespace Backend.Features.Activities.Services;

public interface IActivityService
{
    Task<Guid> CreateAsync(Guid moduleId, CreateActivityDto dto);

    Task<List<ViewActivityDto>> GetByModuleAsync(Guid moduleId);

    Task<bool> UpdateAsync(Guid id, UpdateActivityDto dto);

    Task<bool> DeleteAsync(Guid id);
}
