namespace CookTime.ViewComponents;

using System;
using System.Collections.Generic;

public class PagedResult<T>
    where T : class
{
    public required List<T> Results { get; set; }

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

    public required int CurrentPage { get; set; }

    public required int PageCount { get; set; }

    public required int PageSize { get; set; }

    public required int RowCount { get; set; }

    public int FirstRowOnPage => (this.CurrentPage - 1) * this.PageSize + 1;

    public int LastRowOnPage => Math.Min(this.CurrentPage * this.PageSize, this.RowCount);
}