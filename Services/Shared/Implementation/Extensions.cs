using babe_algorithms.ViewComponents;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

namespace GustavoTech.Implementation;

public static class Extensions
{
    public static (string cssClass, string text) GetElapsedTimeDisplay(this TimeSpan timespan)
    {
        var daysAgo = Math.Floor(timespan.TotalDays);
        var weeksAgo = Math.Floor(timespan.TotalDays / 7);
        if (daysAgo <= 3)
        {
            return ("text-success", "New!");
        }
        else if (daysAgo < 14)
        {
            return ("text-secondary", $"{daysAgo} days ago");
        }
        else
        {
            return ("text-secondary", $"{weeksAgo} weeks ago");
        }
    }
}

public static class IQueryableExtensions
{
    public static async Task<PagedResult<T>> GetPagedAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize)
        where T : class
    {
        var result = new PagedResult<T>
        {
            CurrentPage = page,
            PageSize = pageSize,
            RowCount = query.Count(),
        };

        var pageCount = (double)result.RowCount / Math.Min(pageSize, 100);
        result.PageCount = (int)Math.Ceiling(pageCount);
        var skip = (page - 1) * pageSize;
        var subset = query.Skip(skip).Take(pageSize).Cast<T>();
        // Work?
        result.Results = query switch {
            List<T> => subset.ToList(),
            IQueryable<T> => await subset.ToListAsync(),
        };
        return result;
    }

    public static PagedResult<T> GetPaged<T>(
        this IEnumerable<T> results,
        int page,
        int pageSize)
        where T : class
    {
        var result = new PagedResult<T>
        {
            CurrentPage = page,
            PageSize = pageSize,
            RowCount = results.Count(),
        };

        var pageCount = (double)result.RowCount / Math.Min(pageSize, 100);
        result.PageCount = (int)Math.Ceiling(pageCount);
        var skip = (page - 1) * pageSize;
        result.Results = results.Skip(skip).Take(pageSize).Cast<T>().ToList();
        return result;
    }
}

public static class QueryStringExtensions
{
    public static string SetKey(this string query, string key, string value)
    {
        var q = QueryHelpers.ParseQuery(query);
        var items = q.SelectMany(
            x => x.Value,
            (col, val) => new KeyValuePair<string, string>(col.Key, val)).ToList();

        // At this point you can remove items if you want
        items.RemoveAll(x => x.Key == key); // Remove all values for key
        items.Add(new KeyValuePair<string, string>(key, value));

        // Use the QueryBuilder to add in new items in a safe way (handles multiples and empty values)
        var qb = new QueryBuilder(items);
        return qb.ToString();
    }
}
