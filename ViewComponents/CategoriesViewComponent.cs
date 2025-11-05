using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using test2.Data;

namespace test2.ViewComponents;

public class CategoriesViewComponent : ViewComponent
{
    private readonly test2Context _context;
    public CategoriesViewComponent(test2Context context)
    {
        _context = context;
    }
    public async Task<IViewComponentResult> InvokeAsync()
    {
        var categories = await _context.Categories.ToListAsync();
        return View(categories);
    }
}