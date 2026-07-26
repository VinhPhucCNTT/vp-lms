using AutoMapper;
using Backend.Api.Core.Entities.Courses;
using Backend.Api.Core.Types;

namespace Backend.Api.Core.Automapper;

public class CourseProfile : Profile
{
    public CourseProfile()
    {
        CreateMap<Course, CourseResponse>()
            .MapSqidId()
            .ForMember(
                d => d.CreatorUsername,
                o => o.MapFrom(x => x.Creator != null ? x.Creator.Username : ""))
            .ForMember(
                d => d.CreatorFullname,
                o => o.MapFrom(x => x.Creator != null ? x.Creator.Fullname : ""))
            .ForMember(
                d => d.EnrollmentCount,
                o => o.MapFrom(x => x.Enrollments.Count));

        CreateMap<Course, CourseSetResponse>()
            .MapSqidId()
            .ForMember(
                d => d.CreatorId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.CreatorId));

    }
}
