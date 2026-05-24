namespace Backend.Common;

public class QueryResponse<T>(
    int pageNumber,
    int pageSize,
    int maxPage,
    List<T> data
)
{
    public int PageNumber { get; set; } = pageNumber;
    public int PageSize { get; set; } = pageSize;
    public int MaxPage { get; set; } = maxPage;
    public List<T> Data { get; set; } = data;
}
