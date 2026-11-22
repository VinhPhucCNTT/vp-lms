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
        CreateMap<long?, string?>()
            .ConvertUsing<NullableSqidTypeConverter>();

        CreateMap<Course, CourseResponse>()
            .MapSqidId();

        CreateMap<Course, CourseDetailResponse>()
            .ForCtorParam(
                "Id",
                o => o.MapFrom(x => x.Id))
            .ForCtorParam(
                "InstructorId",
                o => o.MapFrom(x => x.InstructorId))
            .ForCtorParam(
                "InstructorFirstName",
                o => o.MapFrom(x => x.Instructor != null ? x.Instructor.FirstName : ""))
            .ForCtorParam(
                "InstructorLastName",
                o => o.MapFrom(x => x.Instructor != null ? x.Instructor.LastName : ""))
            .ForCtorParam(
                "EnrollmentCount",
                o => o.MapFrom(x => x.Enrollments.Count))
            .ForCtorParam(
                "EnrollmentOpen",
                o => o.MapFrom(x => x.EnrollmentOpen))
            .ForMember(
                d => d.BackgroundFileId,
                o => o.ConvertUsing<NullableSqidConverter, long?>(x => x.BackgroundFileId));

        CreateMap<Course, CourseSetResponse>()
            .MapSqidId()
            .ForMember(
                d => d.BackgroundFileId,
                o => o.ConvertUsing<NullableSqidConverter, long?>(x => x.BackgroundFileId));

    }
}
