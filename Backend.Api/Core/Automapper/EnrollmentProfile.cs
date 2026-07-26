using AutoMapper;
using Backend.Api.Core.Entities.Courses;
using Backend.Api.Core.Types;

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
