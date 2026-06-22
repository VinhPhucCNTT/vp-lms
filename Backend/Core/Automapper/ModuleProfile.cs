using AutoMapper;
using Backend.Core.Entities.Courses;
using Backend.Core.Types;

namespace Backend.Core.Automapper;

public class ModuleProfile : Profile
{
    public ModuleProfile()
    {
        CreateMap<CourseModule, ModuleResponse>()
            .MapSqidId();

        CreateMap<CourseModule, ModuleDetailResponse>()
            .MapSqidId();

        CreateMap<CourseModule, ModuleSetResponse>()
            .MapSqidId();
    }
}
