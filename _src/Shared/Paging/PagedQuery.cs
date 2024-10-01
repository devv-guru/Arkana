namespace Shared.Paging;

public class PagedQuery
{
    public virtual int Page { get; init; }
    public virtual int PageSize { get; init; }
    public virtual string? SortBy { get; init; }
    public virtual bool SortAscending { get; init; }

    public PagedQuery()
    {
    }

    public PagedQuery(int page, int pageSize, string? sortBy, bool sortAscending)
    {
        Page = page;
        PageSize = pageSize;
        SortBy = sortBy;
        SortAscending = sortAscending;
    }

    public override string ToString()
    {
        var queryString = $"page={Page}&pageSize={PageSize}";

        if (!string.IsNullOrWhiteSpace(SortBy))
            queryString += $"&sortBy={SortBy}";

        queryString += $"&sortAscending={SortAscending.ToString().ToLower()}";

        return queryString;
    }
}