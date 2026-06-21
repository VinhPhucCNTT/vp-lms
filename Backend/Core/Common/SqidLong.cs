using System.Reflection;
using Sqids;

namespace Backend.Core.Common;

public readonly record struct SqidLong(long Value)
{
    public static ValueTask<SqidLong?> BindAsync(
        HttpContext context,
        ParameterInfo parameter)
    {
        var rawValue =
            context.Request.RouteValues[
                parameter.Name!
            ]?.ToString();

        if (string.IsNullOrWhiteSpace(rawValue))
            return ValueTask.FromResult<SqidLong?>(null);

        var sqids =
            context.RequestServices
                .GetRequiredService<SqidsEncoder<long>>();

        var decoded = sqids.Decode(rawValue);

        if (decoded.Count != 1)
            throw new BadHttpRequestException(
                "Invalid ID");

        return ValueTask.FromResult<SqidLong?>(
            new SqidLong(decoded[0]));
    }
}
