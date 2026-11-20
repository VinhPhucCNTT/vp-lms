using AutoMapper;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Assignments;

namespace Backend.Api.Core.Automapper;

public class AssignmentProfile : Profile
{
    public AssignmentProfile()
    {
        CreateMap<AssignmentSubmission, SubmissionResponse>()
            .ForMember(
                d => d.SubmissionId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.AssignmentId))
            .ForMember(
                d => d.AssignmentId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.AssignmentId))
            .ForMember(
                d => d.UserId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.UserId))
            .DisableCtorValidation();

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
