using AutoMapper;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Courses;

namespace Backend.Api.Core.Automapper;

public class CourseProfile : Profile
{
    public CourseProfile()
    {
        CreateMap<long, string>()
            .ConvertUsing<SqidTypeConverter>();

        CreateMap<Course, CourseResponse>()
            .ForCtorParam(
                "Id",
                o => o.MapFrom(x => x.Id))
            .ForCtorParam(
                "CreatorId",
                o => o.MapFrom(x => x.CreatorId))
            .ForCtorParam(
                "CreatorFirstName",
                o => o.MapFrom(x => x.Creator != null ? x.Creator.FirstName : ""))
            .ForCtorParam(
                "CreatorLastName",
                o => o.MapFrom(x => x.Creator != null ? x.Creator.LastName : ""))
            .ForCtorParam(
                "EnrollmentCount",
                o => o.MapFrom(x => x.Enrollments.Count));

        CreateMap<Course, CourseSetResponse>()
            .MapSqidId()
            .ForMember(
                d => d.CreatorId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.CreatorId));

    }
}
