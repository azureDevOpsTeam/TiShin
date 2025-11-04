using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Orders
{
    [Authorize(Roles = "Seller")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public List<Order> Orders { get; set; } = new();

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public void OnGet()
        {
            Orders = _db.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.Id)
                .Take(50)
                .ToList();
        }
    }
}