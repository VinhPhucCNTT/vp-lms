using AutoMapper;
using Backend.Core.Entities.Users;
using Backend.Core.Types;

namespace Backend.Core.Automapper;

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
