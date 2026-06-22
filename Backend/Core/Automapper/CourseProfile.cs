using AutoMapper;
using Backend.Core.Entities.Courses;
using Backend.Core.Types;

namespace Backend.Core.Automapper;

public class CourseProfile : Profile
{
    public CourseProfile()
    {
        CreateMap<Course, CourseResponse>()
            .MapSqidId()
            .ForMember(
                d => d.CreatorId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.Creator.Id))
            .ForMember(
                d => d.CreatorUserName,
                o => o.MapFrom(x => x.Creator.Username));

    }
}
