using AutoMapper;
using Backend.Persistence.Entities.Content;
using Backend.Api.Core.Types;

namespace Backend.Api.Core.Automapper;

public class FileProfile : Profile
{
    public FileProfile()
    {
        CreateMap<FileAsset, FileResponse>()
            .MapSqidId()
            .ForMember(
                d => d.UserId,
                o => o.ConvertUsing<SqidConverter, long>(x => x.UserId));
    }
}
