using AutoMapper;
using Backend.Api.Core.Types;
using Backend.Persistence.Common;
using Backend.Persistence.Entities.Content;
using Backend.Persistence.Entities.Judge;

namespace Backend.Api.Core.Automapper;

public class CodingProblemProfile : Profile
{
    public CodingProblemProfile()
    {
        CreateMap<ProblemTestCase, ProblemTestCaseResponse>()
            .MapSqidId();

        CreateMap<Compiler, CompilerDto>()
            .MapSqidId();
    }
}
