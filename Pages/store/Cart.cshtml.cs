using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.Store;

public class CartModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CartModel(ApplicationDbContext db) { _db = db; }

    public IList<CartItem> Items { get; set; } = new List<CartItem>();

    public async Task OnGet()
    {
        var username = User?.Identity?.Name;
        var user = !string.IsNullOrWhiteSpace(username)
            ? await _db.Users.FirstOrDefaultAsync(u => u.UserName == username)
            : null;
        var userId = user?.Id ?? 0;
        var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart != null)
        {
            Items = await _db.CartItems
                .Include(ci => ci.Product)
                .Include(ci => ci.ProductSize).ThenInclude(ps => ps.Size)
                .Include(ci => ci.ProductColor).ThenInclude(pc => pc.Color)
                .Include(ci => ci.ProductMaterial).ThenInclude(pm => pm.Material)
                .Where(ci => ci.CartId == cart.Id)
                .ToListAsync();
        }
    }

    public async Task<IActionResult> OnPostRemove(int id)
    {
        var username = User?.Identity?.Name;
        var user = !string.IsNullOrWhiteSpace(username)
            ? await _db.Users.FirstOrDefaultAsync(u => u.UserName == username)
            : null;
        var userId = user?.Id ?? 0;

        var item = await _db.CartItems
            .Include(ci => ci.Cart)
            .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.UserId == userId);
        if (item != null)
        {
            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage();
    }
}