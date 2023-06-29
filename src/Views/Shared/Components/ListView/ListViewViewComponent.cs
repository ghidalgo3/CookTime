namespace babe_algorithms.ViewComponents;

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

public class ListViewViewComponent : ViewComponent
{
    public ListViewViewComponent(ILogger<ListViewViewComponent> logger)
    {
        this.Logger = logger;
    }

    public PagedResult<object> Data { get; set; }

    public string PartialView { get; set; }

    public ILogger<ListViewViewComponent> Logger { get; }

    public async Task<IViewComponentResult> InvokeAsync(
        PagedResult<object> query,
        string partialView)
    {
        this.Data = query;
        this.PartialView = partialView;
        return await Task.FromResult(this.View(this));
    }
}