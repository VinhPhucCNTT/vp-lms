using AutoMapper;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Courses;

namespace Backend.Api.Core.Automapper;

public class ResourceProfile : Profile
{
    public ResourceProfile()
    {
        CreateMap<CourseResource, ResourceResponse>()
            .MapSqidId();

        CreateMap<CourseResource, ResourceDetailResponse>()
            .ForCtorParam("Id", o => o.MapFrom(x => x.Id))
            .ForCtorParam("Type", o => o.MapFrom(x => x.Type))
            .ForCtorParam("Title", o => o.MapFrom(x => x.Title))
            .ForCtorParam("OrderIndex", o => o.MapFrom(x => x.OrderIndex))
            .ForCtorParam("AvailableFrom", o => o.MapFrom(_ => (DateTime?)null))
            .ForCtorParam("AvailableUntil", o => o.MapFrom(_ => (DateTime?)null))
            .ForCtorParam("CreatedAt", o => o.MapFrom(x => x.CreatedAt))
            .ForCtorParam("UpdatedAt", o => o.MapFrom(x => x.UpdatedAt));

        CreateMap<Lesson, LessonInfo>();
        CreateMap<Assignment, AssignmentInfo>();
        CreateMap<Assessment, AssessmentInfo>();
        CreateMap<CodingProblem, CodingProblemInfo>();
    }
}
