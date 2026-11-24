namespace Backend.Api.Core.Common;

public record PageRequest(int PageNumber = 1, int PageSize = 10)
{
    public static bool TryCreate(string? pageNumber, string? pageSize, out PageRequest page)
    {
        var number = 1;
        var size = 10;
        page = new PageRequest();
        if ((pageNumber is not null && !int.TryParse(pageNumber, out number)) ||
            (pageSize is not null && !int.TryParse(pageSize, out size)))
            return false;

        page = new PageRequest(number, size);
        return page.IsValid();
    }

    // Bound the page size and prevent integer overflow in Skip calculations.
    public bool IsValid() => PageNumber >= 1 && PageSize >= 1 && PageSize <= 100 &&
        ((long)PageNumber - 1) * PageSize <= int.MaxValue;
}
