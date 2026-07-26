using AutoMapper;
using Backend.Api.Core.Entities.Courses;
using Backend.Api.Core.Entities.Content;
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
