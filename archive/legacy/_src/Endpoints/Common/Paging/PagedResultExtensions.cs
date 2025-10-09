using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace Endpoints.Common.Paging;

public static class PagedResultExtensions
{
    // Method to return a paged result in any collection type (List, Array, IEnumerable)
    public static async Task<PagedResult<TResult, List<TResult>>> ToPagedListAsync<T, TResult>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        Expression<Func<T, TResult>> projection,
        CancellationToken cancellationToken)
    {
        // Get the total count of items
        var totalItems = await query.CountAsync(cancellationToken);

        // Apply paging
        var pagedItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(projection)
            .ToListAsync(cancellationToken);

        // Return as PagedResult with List<TResult>
        return new PagedResult<TResult, List<TResult>>(pagedItems, totalItems, page, pageSize);
    }
    
    public static async Task<PagedResult<TResult, List<TResult>>> ToPagedListAsync<T, TResult>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        Func<T, TResult> projection,
        CancellationToken cancellationToken)
    {
        // Get the total count of items
        var totalItems = await query.CountAsync(cancellationToken);

        // Apply paging
        var pagedItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
              .Select(entity => projection(entity))
            .ToListAsync(cancellationToken);

        // Return as PagedResult with List<TResult>
        return new PagedResult<TResult, List<TResult>>(pagedItems, totalItems, page, pageSize);
    }

    public static async Task<PagedResult<TResult, TResult[]>> ToPagedArrayAsync<T, TResult>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        Expression<Func<T, TResult>> projection,
        CancellationToken cancellationToken)
    {
        // Get the total count of items
        var totalItems = await query.CountAsync(cancellationToken);

        var pagedItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(projection)
            .ToArrayAsync(cancellationToken);

        // Return as PagedResult with array TResult[]
        return new PagedResult<TResult, TResult[]>(pagedItems, totalItems, page, pageSize);
    }

    public static async Task<PagedResult<TResult, TResult[]>> ToPagedArrayAsync<T, TResult>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        Func<T, TResult> projection,
        CancellationToken cancellationToken)
    {
        // Get the total count of items
        var totalItems = await query.CountAsync(cancellationToken);

        var pagedItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(entity => projection(entity))
            .ToArrayAsync(cancellationToken);

        // Return as PagedResult with array TResult[]
        return new PagedResult<TResult, TResult[]>(pagedItems, totalItems, page, pageSize);
    }

    public static async Task<PagedResult<TResult, IEnumerable<TResult>>> ToPagedEnumerableAsync<T, TResult>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        Expression<Func<T, TResult>> projection,
        CancellationToken cancellationToken)
    {
        // Get the total count of items
        var totalItems = await query.CountAsync(cancellationToken);

        // Apply paging
        var pagedItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(projection)
            .ToListAsync(cancellationToken); // using List but returning IEnumerable

        // Return as PagedResult with IEnumerable<TResult>
        return new PagedResult<TResult, IEnumerable<TResult>>(pagedItems, totalItems, page, pageSize);
    }
    
    public static async Task<PagedResult<TResult, IEnumerable<TResult>>> ToPagedEnumerableAsync<T, TResult>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        Func<T, TResult> projection,
        CancellationToken cancellationToken)
    {
        // Get the total count of items
        var totalItems = await query.CountAsync(cancellationToken);

        // Apply paging
        var pagedItems = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(entity => projection(entity))
            .ToListAsync(cancellationToken); // using List but returning IEnumerable

        // Return as PagedResult with IEnumerable<TResult>
        return new PagedResult<TResult, IEnumerable<TResult>>(pagedItems, totalItems, page, pageSize);
    }
}