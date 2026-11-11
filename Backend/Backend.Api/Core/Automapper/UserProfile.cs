using AutoMapper;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Users;

namespace Backend.Api.Core.Automapper;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserResponse>()
            .MapSqidId()
            .ForMember(
                d => d.AvatarFileId,
                o => o.ConvertUsing<NullableSqidConverter, long?>(x => x.AvatarFileId));

        CreateMap<User, UserDetailResponse>()
            .MapSqidId()
            .ForMember(
                d => d.AvatarFileId,
                o => o.ConvertUsing<NullableSqidConverter, long?>(x => x.AvatarFileId));

        CreateMap<User, UserSetResponse>()
            .MapSqidId()
            .ForMember(
                d => d.AvatarFileId,
                o => o.ConvertUsing<NullableSqidConverter, long?>(x => x.AvatarFileId));

        CreateMap<User, UserSetResponse>()
            .MapSqidId()
            .ForMember(
                d => d.AvatarFileId,
                o => o.ConvertUsing<NullableSqidConverter, long?>(x => x.AvatarFileId));
    }
}
