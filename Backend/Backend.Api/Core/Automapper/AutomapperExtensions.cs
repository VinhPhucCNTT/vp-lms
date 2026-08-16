using AutoMapper;
using Backend.Persistence.Common;
using Backend.Api.Core.Types;

namespace Backend.Api.Core.Automapper;

public static class AutomapperExtensions
{
    public static IMappingExpression<TEntity, TDto>
        MapSqidId<TEntity, TDto>(
            this IMappingExpression<TEntity, TDto> map)
        where TEntity : BaseEntity
        where TDto : IEntityResponse
    {
        return map.ForMember(
            d => d.Id,
            o => o.ConvertUsing<SqidConverter, long>(
                s => s.Id));
    }
}
