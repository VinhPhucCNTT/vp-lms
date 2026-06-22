using AutoMapper;
using Backend.Core.Entities.Courses;
using Backend.Core.Types;

namespace Backend.Core.Automapper;

public class ResourceProfile : Profile
{
    public ResourceProfile()
    {
        CreateMap<ModuleResource, ResourceResponse>()
            .MapSqidId();

        CreateMap<ModuleResource, ResourceDetailResponse>()
            .MapSqidId();

        CreateMap<ModuleResource, ResourceSetResponse>()
            .MapSqidId();
    }
}
