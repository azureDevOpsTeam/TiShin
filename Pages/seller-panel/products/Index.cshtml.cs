using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Products
{
    [Authorize(Roles = "Seller")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) { _db = db; }

        public IList<Product> Items { get; set; } = new List<Product>();

        public async Task OnGet()
        {
            Items = await _db.Products
                .AsNoTracking()
                .OrderByDescending(p => p.Id)
                .Take(100)
                .ToListAsync();
        }
    }
}