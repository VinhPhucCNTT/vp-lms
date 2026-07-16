using AutoMapper;
using Backend.Core.Entities.Courses;
using Backend.Core.Entities.Resources;
using Backend.Core.Types;

namespace Backend.Core.Automapper;

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
