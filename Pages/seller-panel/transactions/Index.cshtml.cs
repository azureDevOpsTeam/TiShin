using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;
using TiShinShop.Extenssions.Enums;

namespace TiShinShop.Pages.SellerPanel.Transactions;

[Authorize(Roles = "Seller")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) { _db = db; }

    public List<Order> Transactions { get; set; } = new();

    // Filters
    [BindProperty(SupportsGet = true)] public string Status { get; set; } = "all";
    [BindProperty(SupportsGet = true)] public string? Q { get; set; }
    [BindProperty(SupportsGet = true)] public string? FromDate { get; set; }
    [BindProperty(SupportsGet = true)] public int PageSize { get; set; } = 50;

    // Stats
    public int TotalCount { get; set; }
    public decimal TotalSuccessAmount { get; set; }
    public int FailedCount { get; set; }
    public decimal AverageAmount { get; set; }

    public async Task OnGet()
    {
        // Base query
        var query = _db.Orders
            .Include(o => o.User)
            .Include(o => o.ShippingAddress)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(Q))
        {
            query = query.Where(o =>
                (!string.IsNullOrEmpty(o.TrackingCode) && o.TrackingCode.Contains(Q)) ||
                (o.ShippingAddress != null && (
                    (!string.IsNullOrEmpty(o.ShippingAddress.Phone) && o.ShippingAddress.Phone.Contains(Q)) ||
                    (!string.IsNullOrEmpty(o.ShippingAddress.FullName) && o.ShippingAddress.FullName.Contains(Q))
                )) ||
                (o.User != null && (
                    (!string.IsNullOrEmpty(o.User.UserName) && o.User.UserName.Contains(Q)) ||
                    (!string.IsNullOrEmpty(o.User.PhoneNumber) && o.User.PhoneNumber.Contains(Q))
                ))
            );
        }

        if (!string.IsNullOrWhiteSpace(FromDate) && DateTime.TryParse(FromDate, out var from))
        {
            var dateOnly = from.Date;
            query = query.Where(o => o.CreatedAt.Date >= dateOnly);
        }

        switch (Status?.ToLowerInvariant())
        {
            case "success":
                query = query.Where(o => o.Status == OrderStatus.Paid);
                break;
            case "pending":
                query = query.Where(o => o.Status == OrderStatus.Pending);
                break;
            case "failed":
                query = query.Where(o => o.Status == OrderStatus.Failed);
                break;
            case "cancelled":
                query = query.Where(o => o.Status == OrderStatus.Canceled);
                break;
        }

        Transactions = await query
            .OrderByDescending(o => o.Id)
            .Take(PageSize)
            .ToListAsync();

        // Stats (global across orders, not only page-size limited)
        var allOrders = _db.Orders.AsNoTracking();
        TotalCount = await allOrders.CountAsync();
        TotalSuccessAmount = await allOrders.Where(o => o.Status == OrderStatus.Paid)
            .SumAsync(o => (decimal?)o.FinalPrice) ?? 0m;
        FailedCount = await allOrders.CountAsync(o => o.Status == OrderStatus.Failed);
        var avg = await allOrders.AverageAsync(o => (double)o.FinalPrice);
        AverageAmount = (decimal)avg;
    }

    public async Task<IActionResult> OnPostExport()
    {
        // Export CSV containing recent transactions with applied filters
        var query = _db.Orders
            .Include(o => o.User)
            .Include(o => o.ShippingAddress)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(Q))
        {
            query = query.Where(o =>
                (!string.IsNullOrEmpty(o.TrackingCode) && o.TrackingCode.Contains(Q)) ||
                (o.ShippingAddress != null && (
                    (!string.IsNullOrEmpty(o.ShippingAddress.Phone) && o.ShippingAddress.Phone.Contains(Q)) ||
                    (!string.IsNullOrEmpty(o.ShippingAddress.FullName) && o.ShippingAddress.FullName.Contains(Q))
                )) ||
                (o.User != null && (
                    (!string.IsNullOrEmpty(o.User.UserName) && o.User.UserName.Contains(Q)) ||
                    (!string.IsNullOrEmpty(o.User.PhoneNumber) && o.User.PhoneNumber.Contains(Q))
                ))
            );
        }

        if (!string.IsNullOrWhiteSpace(FromDate) && DateTime.TryParse(FromDate, out var from))
        {
            var dateOnly = from.Date;
            query = query.Where(o => o.CreatedAt.Date >= dateOnly);
        }

        switch (Status?.ToLowerInvariant())
        {
            case "success":
                query = query.Where(o => o.Status == OrderStatus.Paid);
                break;
            case "pending":
                query = query.Where(o => o.Status == OrderStatus.Pending);
                break;
            case "failed":
                query = query.Where(o => o.Status == OrderStatus.Failed);
                break;
            case "cancelled":
                query = query.Where(o => o.Status == OrderStatus.Canceled);
                break;
        }

        var rows = await query.OrderByDescending(o => o.Id).Take(1000).ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("TransactionId,Date,Customer,Phone,Amount,Status");
        foreach (var o in rows)
        {
            var txId = string.IsNullOrWhiteSpace(o.TrackingCode) ? $"TX-{o.Id}" : o.TrackingCode;
            var date = o.CreatedAt.ToString("yyyy/MM/dd HH:mm");
            var name = o.ShippingAddress?.FullName ?? o.User?.UserName ?? "";
            var phone = o.ShippingAddress?.Phone ?? o.User?.PhoneNumber ?? "";
            var amount = o.FinalPrice.ToString("N0");
            var status = o.Status.ToString();
            sb.AppendLine($"{txId},{date},{name},{phone},{amount},{status}");
        }

        var bytes = Encoding.UTF8.GetBytes(sb.ToString());
        return File(bytes, "text/csv", "transactions.csv");
    }
}