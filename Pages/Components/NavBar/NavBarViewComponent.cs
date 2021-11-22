using babe_algorithms.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
namespace babe_algorithms.ViewComponents;

public class NavBarViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public NavBarViewComponent(ApplicationDbContext context)
    {
        this._context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        return await Task.Run(() => this.View(this));
    }
}