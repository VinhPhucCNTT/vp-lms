using Backend.Data.Repositories;
using Backend.Data.UnitOfWork;
using Backend.Features.Activities;
using Backend.Features.Activities.Dtos;
using Backend.Features.Modules.Dtos;
using Backend.Models.Courses;

namespace Backend.Features.Modules.Services;

public class ModuleService(
    IModuleRepository moduleRepo,
    IActivityRepository activityRepo,
    IUnitOfWork uow) : IModuleService
{
    private readonly IModuleRepository _moduleRepo = moduleRepo;
    private readonly IActivityRepository _activityRepo = activityRepo;
    private readonly IUnitOfWork _uow = uow;

    public async Task<Guid> CreateAsync(
        Guid courseId,
        CreateModuleDto dto)
    {
        var module = new CourseModule
        {
            CourseId = courseId,
            Title = dto.Title,
            OrderIndex = dto.OrderIndex
        };

        _moduleRepo.Add(module);

        await _uow.SaveChangesAsync();

        return module.Id;
    }

    public async Task<List<ViewModuleDto>> GetByCourseAsync(Guid courseId)
    {
        var modules = await _moduleRepo.GetByCourseIdAsync(courseId);

        var list = new List<ViewModuleDto>();
        foreach (var module in modules) {
            var activities = await _activityRepo.GetByModuleIdAsync(module.Id);

            list.Add(new ViewModuleDto(
                module.Id,
                module.Title,
                module.OrderIndex,
                activities
                    .Select(a => new ViewActivityDto(
                        a.Id,
                        a.Title,
                        a.Type,
                        a.OrderIndex,
                        a.IsPublished,
                        a.AvailableFrom,
                        a.AvailableUntil,
                        a.GetResourceId()
                    )).ToList()
            ));
        }

        return list;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateModuleDto dto)
    {
        var module = await _moduleRepo.GetByIdAsync(id);
        if (module is null)
            return false;

        module.Title = dto.Title;
        module.OrderIndex = dto.OrderIndex;

        _moduleRepo.Update(module);

        await _uow.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var module = await _moduleRepo.GetByIdAsync(id);
        if (module is null)
            return false;

        _moduleRepo.Remove(module);

        await _uow.SaveChangesAsync();
        return true;
    }
}
