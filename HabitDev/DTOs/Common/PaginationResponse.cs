using Microsoft.EntityFrameworkCore;

namespace HabitDev.DTOs.Common;

public sealed record PaginationResponse<T> : ICollectionResponse<T>
{
    public List<T> Items { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);

    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public List<LinkDto> Links { get; set; } = [];

    public static async Task<PaginationResponse<T>> CreateAsync(
        IQueryable<T> query,
        int page,
        int pageSize)
    {
        int totalCount = await query.CountAsync();

        List<T> items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
       
        return new PaginationResponse<T>()
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }
#pragma warning disable CA2008
    public static async Task<PaginationResponse<TResult>> CreateAsync<TResult>(
        IQueryable<T> query,
        int page,
        int pageSize,
        Func<T, TResult> map)
    {
        int totalCount = await query.CountAsync();


        List<TResult> items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync()
            .ContinueWith(t => t.Result.Select(map).ToList());


        return new PaginationResponse<TResult>
        {
            Items      = items,
            Page       = page,
            PageSize   = pageSize,
            TotalCount = totalCount
        };
    }

}
#pragma warning restore CA2008
