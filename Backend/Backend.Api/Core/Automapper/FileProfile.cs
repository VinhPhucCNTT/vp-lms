using AutoMapper;
using Backend.Api.Core.Types;
using Backend.Persistence.Entities.Content;

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
