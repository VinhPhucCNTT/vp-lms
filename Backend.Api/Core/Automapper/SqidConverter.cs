using AutoMapper;
using Sqids;

namespace Backend.Api.Core.Automapper;

public class SqidConverter(SqidsEncoder<long> sqids) : IValueConverter<long, string>
{
    private readonly SqidsEncoder<long> _sqids = sqids;

    public string Convert(
        long sourceMember,
        ResolutionContext context)
    {
        return _sqids.Encode(sourceMember);
    }
}
