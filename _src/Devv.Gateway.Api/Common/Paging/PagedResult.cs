namespace Devv.Gateway.Api.Common.Paging;

public class PagedResult<T, TCollection> where TCollection : IEnumerable<T>
{
    public TCollection Items { get; set; }
    public int TotalItems { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);

    public PagedResult(TCollection items, int totalItems, int page, int pageSize)
    {
        Items = items;
        TotalItems = totalItems;
        Page = page;
        PageSize = pageSize;
    }
}