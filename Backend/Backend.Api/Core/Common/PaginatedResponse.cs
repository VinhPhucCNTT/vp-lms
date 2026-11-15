namespace Backend.Api.Core.Common;

public record PaginatedResponse<T>(
    int PageNumber,
    int PageSize,
    int MaxCount,
    List<T> Data
)
{
    public int PageNumber { get; init; } = PageNumber;
    public int PageSize { get; init; } = PageSize;
    public int MaxCount { get; init; } = MaxCount;
    public List<T> Data { get; init; } = Data;
}
