using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Restaurant.Web.Data;

namespace Restaurant.Web.Pages.Cart;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<IdentityUser> _userManager;

    public IndexModel(ApplicationDbContext dbContext, UserManager<IdentityUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public record CartLine(MenuItem MenuItem, int Quantity);
    public IList<CartLine> Items { get; set; } = new List<CartLine>();
    public decimal Total { get; set; }

    public async Task OnGetAsync()
    {
        var items = ReadCart();
        if (items.Count == 0) return;
        var ids = items.Keys.ToList();
        var menuItems = await _dbContext.MenuItems.Where(m => ids.Contains(m.Id)).ToListAsync();
        Items = menuItems.Select(mi => new CartLine(mi, items[mi.Id])).ToList();
        Total = Items.Sum(i => i.MenuItem.Price * i.Quantity);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var items = ReadCart();
        if (items.Count == 0) return RedirectToPage();

        var ids = items.Keys.ToList();
        var menuItems = await _dbContext.MenuItems.Where(m => ids.Contains(m.Id)).ToListAsync();
        var userId = _userManager.GetUserId(User)!;

        var order = new Order
        {
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            Status = "Pending",
            Items = new List<OrderItem>()
        };

        foreach (var mi in menuItems)
        {
            var qty = items[mi.Id];
            order.Items.Add(new OrderItem
            {
                MenuItemId = mi.Id,
                Quantity = qty,
                UnitPrice = mi.Price
            });
        }
        order.TotalAmount = order.Items.Sum(i => i.UnitPrice * i.Quantity);
        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        // Clear cart
        HttpContext.Session.Remove("cart");
        return RedirectToPage("/Index");
    }

    private Dictionary<int, int> ReadCart()
    {
        var cart = HttpContext.Session.GetString("cart");
        return string.IsNullOrEmpty(cart)
            ? new Dictionary<int, int>()
            : System.Text.Json.JsonSerializer.Deserialize<Dictionary<int, int>>(cart!)!;
    }
}
