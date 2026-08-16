using AutoMapper;
using Backend.Persistence.Entities.Assessments;
using Backend.Api.Core.Types;

namespace Backend.Api.Core.Automapper;

public class AssessmentProfile : Profile
{
    public AssessmentProfile()
    {
        CreateMap<Question, QuestionResponse>()
            .MapSqidId()
            .ForMember(
                d => d.BankId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.QuestionBankId));
    }
}
