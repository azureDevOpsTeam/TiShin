using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Coupons
{
    [Authorize(Roles = "Seller")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) { _db = db; }

        public List<Coupon> Items { get; set; } = new();

        public async Task OnGet()
        {
            Items = await _db.Coupons.OrderByDescending(c => c.Id).ToListAsync();
        }
    }
}