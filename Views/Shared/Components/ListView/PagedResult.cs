namespace babe_algorithms.ViewComponents;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;

public class PagedResult<T> : PagedResultBase
    where T : class
{
    public PagedResult()
    {
        this.Results = new List<T>();
    }

    public IList<T> Results { get; set; }

    public Func<int, string> Link { get; set; }
}

public abstract class PagedResultBase
{
    public int CurrentPage { get; set; }

    public int PageCount { get; set; }

    public int PageSize { get; set; }

    public int RowCount { get; set; }

    public int FirstRowOnPage => (this.CurrentPage - 1) * this.PageSize + 1;

    public int LastRowOnPage => Math.Min(this.CurrentPage * this.PageSize, this.RowCount);
}

public static class IQueryableExtensions
{
    public static PagedResult<object> GetPaged<T>(
        this IEnumerable<T> query,
        int page,
        int pageSize,
        Func<int, string> link)
        where T : class
    {
        var result = new PagedResult<object>
        {
            CurrentPage = page,
            PageSize = pageSize,
            RowCount = query.Count(),
            Link = link,
        };

        var pageCount = (double)result.RowCount / Math.Min(pageSize, 100);
        result.PageCount = (int)Math.Ceiling(pageCount);
        var skip = (page - 1) * pageSize;
        result.Results = query.Skip(skip).Take(pageSize).Cast<object>().ToList();
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