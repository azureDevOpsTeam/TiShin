using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Extenssions.Enums;

namespace TiShinShop.Pages.SellerPanel;

[Authorize(Roles = "Seller")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;

    public IndexModel(ApplicationDbContext db)
    {
        _db = db;
    }

    public int TotalOrders { get; set; }
    public int PendingCount { get; set; }
    public int PaidCount { get; set; }
    public int ShippedCount { get; set; }
    public int DeliveredCount { get; set; }
    public int CanceledCount { get; set; }
    public int FailedCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public int TodayOrders { get; set; }
    public decimal TodayRevenue { get; set; }

    public async Task OnGet()
    {
        var orders = _db.Orders.AsNoTracking();
        TotalOrders = await orders.CountAsync();
        TotalRevenue = await orders.SumAsync(o => (decimal?)o.TotalPrice) ?? 0m;

        PendingCount = await orders.CountAsync(o => o.Status == OrderStatus.Pending);
        PaidCount = await orders.CountAsync(o => o.Status == OrderStatus.Paid);
        ShippedCount = await orders.CountAsync(o => o.Status == OrderStatus.Shipped);
        DeliveredCount = await orders.CountAsync(o => o.Status == OrderStatus.Delivered);
        CanceledCount = await orders.CountAsync(o => o.Status == OrderStatus.Canceled);
        FailedCount = await orders.CountAsync(o => o.Status == OrderStatus.Failed);

        var today = System.DateTime.Today;
        TodayOrders = await orders.CountAsync(o => o.CreatedAt.Date == today);
        TodayRevenue = await orders.Where(o => o.CreatedAt.Date == today).SumAsync(o => (decimal?)o.TotalPrice) ?? 0m;
    }
}