using AutoMapper;
using Backend.Api.Core.Entities.Courses;
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
