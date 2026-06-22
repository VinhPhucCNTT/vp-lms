using AutoMapper;
using Backend.Core.Common.Models;
using Backend.Core.Types;

namespace Backend.Core.Automapper;

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
