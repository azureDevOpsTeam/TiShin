using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;
using TiShinShop.Extenssions.Enums;

namespace TiShinShop.Pages.Store;

public class PaymentModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public PaymentModel(ApplicationDbContext db) { _db = db; }

    public Order? Order { get; set; }

    public async Task<IActionResult> OnGet(int id)
    {
        Order = await _db.Orders.Include(o => o.Coupon).FirstOrDefaultAsync(o => o.Id == id);
        if (Order == null) return NotFound();
        return Page();
    }

    public async Task<IActionResult> OnPost(int id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        order.Status = OrderStatus.Paid;
        await _db.SaveChangesAsync();
        return RedirectToPage("/store/SuccessPayment", new { id });
    }
}