using Microsoft.EntityFrameworkCore;

namespace Maliev.Entities.ViewModels;

/// <summary>Retains legacy pagination metadata and provider-backed query behavior.</summary>
/// <typeparam name="T">The source item type.</typeparam>
public class PaginatedListWebApi<T>
{
    /// <summary>Creates an empty page with zero-valued metadata.</summary>
    public PaginatedListWebApi()
    {
    }

    /// <summary>Copies items and preserves the supplied count and page index.</summary>
    /// <param name="items">The items to copy.</param>
    /// <param name="count">The total record count.</param>
    /// <param name="pageIndex">The page index.</param>
    /// <param name="pageSize">The page size used to calculate total pages.</param>
    public PaginatedListWebApi(List<T> items, int count, int pageIndex, int pageSize)
    {
        PageIndex = pageIndex;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        TotalRecords = count;
        Items.AddRange(items);
    }

    /// <summary>Gets whether the page index is below the total page count.</summary>
    public bool HasNextPage => PageIndex < TotalPages;

    /// <summary>Gets whether the page index is greater than one.</summary>
    public bool HasPreviousPage => PageIndex > 1;

    /// <summary>Gets or sets the items, initially an empty list.</summary>
    public List<T> Items { get; set; } = [];

    /// <summary>Gets or sets the page index.</summary>
    public int PageIndex { get; set; }

    /// <summary>Gets or sets the total page count.</summary>
    public int TotalPages { get; set; }

    /// <summary>Gets or sets the total record count.</summary>
    public int TotalRecords { get; set; }

    /// <summary>Counts the provider query, then fetches the requested page.</summary>
    /// <param name="source">The provider-backed source query.</param>
    /// <param name="pageIndex">The requested page index, clamped to at least one.</param>
    /// <param name="pageSize">The page size passed to the provider without additional validation.</param>
    /// <returns>The page, or null when the source count is zero.</returns>
    public static async Task<PaginatedListWebApi<T>?> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
    {
        pageIndex = Math.Max(1, pageIndex);
        var count = await source.CountAsync();
        if (count == 0)
        {
            return null;
        }

        var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedListWebApi<T>(items, count, pageIndex, pageSize);
    }
}
