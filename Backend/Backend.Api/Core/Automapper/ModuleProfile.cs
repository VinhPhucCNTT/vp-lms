using AutoMapper;
using Backend.Persistence.Entities.Courses;
using Backend.Api.Core.Types;

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
