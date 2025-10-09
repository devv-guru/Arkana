using System.Linq.Expressions;

namespace Endpoints.Common.Paging;

public static class QueryExtensions
{
    // Paging and Sorting extension
    public static IQueryable<T> Sort<T>(
        this IQueryable<T> query,
        string? sortBy,
        bool sortAscending)
    {
        // Apply sorting logic if sortBy is provided
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = ApplySorting(query, sortBy, sortAscending);
        }

        // Apply paging
        return query;
    }

    // Projection extension
    public static IQueryable<TResult> ProjectTo<T, TResult>(
        this IQueryable<T> query,
        Expression<Func<T, TResult>> projection)
    {
        return query.Select(projection);
    }

    public static IQueryable<TResult> ProjectTo<T, TResult>(
        this IQueryable<T> query,
        Func<T, TResult> projection)
    {
        return query.Select(entity => projection(entity));
    }

    // Helper method to generate search expressions dynamically
    private static Expression<Func<T, bool>> GetSearchExpression<T>(Expression<Func<T, object>>[] fields,
        string searchTerm)
    {
        var parameter = Expression.Parameter(typeof(T), "x");
        Expression? body = null;

        foreach (var field in fields)
        {
            var memberExpression = Expression.Convert(field.Body, typeof(string));
            var containsMethod = typeof(string).GetMethod(nameof(string.Contains), new[] { typeof(string) });
            var searchExpression = Expression.Call(memberExpression, containsMethod,
                Expression.Constant(searchTerm, typeof(string)));

            body = body == null ? searchExpression : Expression.OrElse(body, searchExpression);
        }

        return Expression.Lambda<Func<T, bool>>(body!, parameter);
    }

    // Helper method to apply sorting dynamically
    public static IQueryable<T> ApplySorting<T>(IQueryable<T> query, string sortBy, bool ascending)
    {
        var param = Expression.Parameter(typeof(T), "x");
        var property = typeof(T).GetProperty(sortBy);

        if (property == null)
            throw new ArgumentException($"No property '{sortBy}' found on type '{typeof(T).Name}'");

        var memberExpression = Expression.Property(param, property);
        var lambda = Expression.Lambda(memberExpression, param);

        string methodName = ascending ? "OrderBy" : "OrderByDescending";
        var resultExpression = Expression.Call(
            typeof(Queryable),
            methodName,
            new Type[] { typeof(T), property.PropertyType },
            query.Expression,
            Expression.Quote(lambda)
        );

        return query.Provider.CreateQuery<T>(resultExpression);
    }
}