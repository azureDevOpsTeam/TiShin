using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.UserPanel.Orders
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        public Order Order { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public List<OrderStatusHistory> History { get; set; } = new();

        public DetailModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            Order = await _db.Orders
                .Include(o => o.ShippingAddress)
                .Include(o => o.Coupon)
                .FirstOrDefaultAsync(o => o.Id == Id && o.UserId == user.Id);
            if (Order == null) return NotFound();
            Items = await _db.OrderItems
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