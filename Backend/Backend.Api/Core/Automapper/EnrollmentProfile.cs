using AutoMapper;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Courses;

namespace Backend.Api.Core.Automapper;

public class EnrollmentProfile : Profile
{
    public EnrollmentProfile()
    {
        CreateMap<Enrollment, EnrollmentResponse>()
            .MapSqidId();

        CreateMap<Enrollment, EnrollmentDetailResponse>()
            .MapSqidId();
    }
}
