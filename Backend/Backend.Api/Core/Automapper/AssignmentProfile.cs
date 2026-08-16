using AutoMapper;
using Backend.Persistence.Entities.Assignments;
using Backend.Persistence.Entities.Content;
using Backend.Api.Core.Types;

namespace Backend.Api.Core.Automapper;

public class AssignmentProfile : Profile
{
    public AssignmentProfile()
    {
        CreateMap<Assignment, AssignmentInfo>();

        CreateMap<AssignmentSubmission, SubmissionResponse>()
            .ForMember(
                d => d.AssignmentId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.AssignmentId))
            .ForMember(
                d => d.UserId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.UserId));

        // CreateMap<AssignmentGrade, AssignmentGradeResponse>()
        //     .MapSqidId()
        //     .ForMember(
        //         d => d.SubmissionId,
        //         o => o.ConvertUsing<SqidConverter, long>(x => x.SubmissionId))
        //     .ForMember(
        //         d => d.GraderId,
        //         o => o.ConvertUsing<SqidConverter, long>(x => x.GraderId));
    }
}
