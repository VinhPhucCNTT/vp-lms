using AutoMapper;
using Backend.Core.Entities.Courses;
using Backend.Core.Types;

namespace Backend.Core.Automapper;

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
