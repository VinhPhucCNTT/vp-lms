using AutoMapper;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Content;

namespace Backend.Api.Core.Automapper;

public class CodingProblemProfile : Profile
{
    public CodingProblemProfile()
    {
        CreateMap<ProblemTestCase, ProblemTestCaseResponse>()
            .MapSqidId();
    }
}
