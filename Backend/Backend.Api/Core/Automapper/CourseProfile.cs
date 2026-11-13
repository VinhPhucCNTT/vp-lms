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

        CreateMap<Course, CourseDetailResponse>()
            .ForCtorParam(
                "Id",
                o => o.MapFrom(x => x.Id))
            .ForCtorParam(
                "CreatorId",
                o => o.MapFrom(x => x.InstructorId))
            .ForCtorParam(
                "CreatorFirstName",
                o => o.MapFrom(x => x.Instructor != null ? x.Instructor.FirstName : ""))
            .ForCtorParam(
                "CreatorLastName",
                o => o.MapFrom(x => x.Instructor != null ? x.Instructor.LastName : ""))
            .ForCtorParam(
                "EnrollmentCount",
                o => o.MapFrom(x => x.Enrollments.Count))
            .ForMember(
                d => d.BackgroundFileId,
                o => o.ConvertUsing<NullableSqidConverter, long?>(x => x.BackgroundFileId));

        CreateMap<Course, CourseSetResponse>()
            .MapSqidId()
            .ForMember(
                d => d.CreatorId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.InstructorId));

    }
}
