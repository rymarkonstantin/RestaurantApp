using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Restaurant.Web.Data;

namespace Restaurant.Web.Pages.Menu;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;

    public IndexModel(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IList<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public async Task OnGetAsync()
    {
        MenuItems = await _dbContext.MenuItems
            .Where(mi => mi.IsAvailable)
            .OrderBy(mi => mi.Name)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostAsync(int menuItemId)
    {
        // Naive cart stored in session for now; later replace with persistent cart
        var cart = HttpContext.Session.GetString("cart");
        var items = string.IsNullOrEmpty(cart) ? new Dictionary<int, int>() : System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(cart!)!;
        if (!items.ContainsKey(menuItemId)) items[menuItemId] = 0;
        items[menuItemId]++;
        HttpContext.Session.SetString("cart", System.Text.Json.JsonSerializer.Serialize(items));
        return RedirectToPage();
    }
}
