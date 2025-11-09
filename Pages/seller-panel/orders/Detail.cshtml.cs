using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;
using TiShinShop.Extenssions;

namespace TiShinShop.Pages.SellerPanel.Orders
{
    [Authorize(Roles = "Seller")]
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        public Order Order { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();
        public List<OrderStatusHistory> History { get; set; } = new();

        public DetailModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            Order = await _db.Orders
                .Include(o => o.User)
                .Include(o => o.ShippingAddress)
                .Include(o => o.Coupon)
                .FirstOrDefaultAsync(o => o.Id == Id);
            if (Order == null) return NotFound();

            OrderItems = await _db.OrderItems
                .Include(i => i.Product)
                .Include(i => i.ProductSize).ThenInclude(ps => ps.Size)
                .Include(i => i.ProductColor).ThenInclude(pc => pc.Color)
                .Include(i => i.ProductMaterial).ThenInclude(pm => pm.Material)
                .Where(i => i.OrderId == Id)
                .ToListAsync();

            History = await _db.OrderStatusHistories
                .Where(h => h.OrderId == Id)
                .OrderBy(h => h.ChangedAt)
                .ToListAsync();

            return Page();
        }
    }
}