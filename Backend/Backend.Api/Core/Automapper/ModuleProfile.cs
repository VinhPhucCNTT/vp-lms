using AutoMapper;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Courses;

namespace Backend.Api.Core.Automapper;

public class ModuleProfile : Profile
{
    public ModuleProfile()
    {
        CreateMap<CourseModule, ModuleResponse>()
            .MapSqidId();

        CreateMap<CourseModule, ModuleSetResponse>()
            .MapSqidId();
    }
}
