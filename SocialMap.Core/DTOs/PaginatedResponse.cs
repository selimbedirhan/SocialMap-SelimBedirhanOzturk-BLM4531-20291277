namespace SocialMap.Core.DTOs;

/// <summary>
/// Sayfalama için generic response wrapper
/// Tüm liste endpoint'lerinde kullanılacak
/// </summary>
public class PaginatedResponse<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
}

/// <summary>
/// Sayfalama isteği için query parametreleri
/// </summary>
public class PaginationQuery
{
    private int _page = 1;
    private int _pageSize = 20;
    private const int MaxPageSize = 100;

    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : (value < 1 ? 1 : value);
    }
}
