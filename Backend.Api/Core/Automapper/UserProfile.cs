using AutoMapper;
using Backend.Api.Core.Entities.Users;
using Backend.Api.Core.Types;

namespace Backend.Api.Core.Automapper;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserResponse>()
            .MapSqidId();

        CreateMap<User, UserDetailResponse>()
            .MapSqidId();

        CreateMap<User, UserSetResponse>()
            .MapSqidId();
    }
}
