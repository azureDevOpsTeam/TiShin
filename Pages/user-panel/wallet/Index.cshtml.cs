using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.UserPanel.Wallet
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public decimal TotalSpent { get; set; }
        public int OrderCount { get; set; }
        public string LastTrackingCode { get; set; } = "-";
        public List<Order> RecentOrders { get; set; } = new();

        public IndexModel(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var query = _db.Orders.Where(o => o.UserId == user.Id);
            OrderCount = await query.CountAsync();
            TotalSpent = await query.SumAsync(o => (decimal?)o.TotalPrice) ?? 0m;
            var lastOrder = await query.OrderByDescending(o => o.Id).FirstOrDefaultAsync();
            if (lastOrder != null && !string.IsNullOrEmpty(lastOrder.TrackingCode))
                LastTrackingCode = lastOrder.TrackingCode;

            RecentOrders = await query.OrderByDescending(o => o.Id).Take(5).ToListAsync();
        }
    }
}