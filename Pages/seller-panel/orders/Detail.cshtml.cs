using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Orders
{
    [Authorize(Roles = "Seller")]
    public class DetailModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        [BindProperty(SupportsGet = true)] public int Id { get; set; }
        public Order? Order { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new();

        public DetailModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public IActionResult OnGet()
        {
            Order = _db.Orders.Include(o => o.User).FirstOrDefault(o => o.Id == Id);
            if (Order == null) return NotFound();
            OrderItems = _db.OrderItems.Include(i => i.Product).Where(i => i.OrderId == Id).ToList();
            return Page();
        }
    }
}