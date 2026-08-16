using AutoMapper;
using Backend.Persistence.Entities.Courses;
using Backend.Persistence.Entities.Content;
using Backend.Api.Core.Types;

namespace Backend.Api.Core.Automapper;

public class ResourceProfile : Profile
{
    public ResourceProfile()
    {
        CreateMap<CourseResource, ResourceResponse>()
            .MapSqidId();

        CreateMap<CourseResource, ResourceDetailResponse>()
            .MapSqidId();

        CreateMap<Lesson, LessonInfo>();
        CreateMap<Assignment, AssignmentInfo>();
        CreateMap<Assessment, AssessmentInfo>();
        CreateMap<CodingProblem, CodingProblemInfo>();
    }
}
