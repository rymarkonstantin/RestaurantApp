using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Restaurant.Web.Data;

namespace Restaurant.Web.Pages.Admin.Orders;

[Authorize(Policy = "RequireAdmin")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    public IndexModel(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IList<Order> Orders { get; set; } = new List<Order>();

    public async Task OnGetAsync()
    {
        Orders = await _dbContext.Orders
            .OrderByDescending(o => o.CreatedAtUtc)
            .ToListAsync();
    }
}
