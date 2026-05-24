namespace Backend.Types;

public class QueryRequest
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 10;
}
