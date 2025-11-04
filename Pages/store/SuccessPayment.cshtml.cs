using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.Store;

public class SuccessPaymentModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public SuccessPaymentModel(ApplicationDbContext db) { _db = db; }

    public string TrackingCode { get; set; } = string.Empty;

    public async Task OnGet(int id)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == id);
        TrackingCode = order?.TrackingCode ?? string.Empty;
    }
}