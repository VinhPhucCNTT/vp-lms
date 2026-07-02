using AutoMapper;
using Backend.Core.Entities.Resources;
using Backend.Core.Entities.Submissions;
using Backend.Core.Types;

namespace Backend.Core.Automapper;

public class AssignmentProfile : Profile
{
    public AssignmentProfile()
    {
        // CreateMap<Course, CourseResponse>()
        //     .MapSqidId()
        //     .ForMember(
        //         d => d.CreatorId,
        //         o => o.ConvertUsing<SqidConverter, long>(x => x.CreatorId))
        //     .ForMember(
        //         d => d.CreatorUserName,
        //         o => o.MapFrom(x => x.Creator != null ? x.Creator.Username : ""));

        CreateMap<Assignment, AssignmentResponse>()
            .MapSqidId();

        CreateMap<AssignmentSubmission, SubmissionResponse>()
            .MapSqidId()
            .ForMember(
                d => d.AssignmentId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.AssignmentId))
            .ForMember(
                d => d.UserId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.UserId));

        CreateMap<AssignmentGrade, AssignmentGradeResponse>()
            .MapSqidId()
            .ForMember(
                d => d.SubmissionId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.SubmissionId))
            .ForMember(
                d => d.GraderId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.GraderId));
    }
}
