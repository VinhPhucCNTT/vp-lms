using AutoMapper;
using Backend.Persistence.Entities.Courses;
using Backend.Api.Core.Types;

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
                "CreatorUsername",
                o => o.MapFrom(x => x.Creator != null ? x.Creator.Username : ""))
            .ForCtorParam(
                "CreatorFullname",
                o => o.MapFrom(x => x.Creator != null ? x.Creator.Fullname : ""))
            .ForCtorParam(
                "Code",
                o => o.MapFrom(x => x.Code))
            .ForCtorParam(
                "Title",
                o => o.MapFrom(x => x.Title))
            .ForCtorParam(
                "Description",
                o => o.MapFrom(x => x.Description))
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
