namespace babe_algorithms.ViewComponents;

using System;
using System.Collections.Generic;

public class PagedResult<T> : PagedResultBase
    where T : class
{
    public PagedResult()
    {
        this.Results = new List<T>();
    }

    public List<T> Results { get; set; }

    public PagedResult<T2> To<T2>(Func<T, T2> mapper)
        where T2 : class
    {
        return new PagedResult<T2>()
        {
            Results = this.Results.Select(mapper).ToList(),
            CurrentPage = this.CurrentPage,
            PageCount = this.PageCount,
            RowCount = this.RowCount,
            PageSize = this.PageSize,
        };
    }
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
