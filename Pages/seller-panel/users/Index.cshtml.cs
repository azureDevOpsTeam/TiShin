using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Users
{
    [Authorize(Roles = "Seller")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public IndexModel(ApplicationDbContext db) { _db = db; }

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        public List<ApplicationUser> Users { get; set; } = new();

        public async Task OnGet()
        {
            var query = _db.Users.AsNoTracking();

            // Filter by role "User"
            var userRoleId = await _db.Roles.Where(r => r.Name == "User").Select(r => r.Id).FirstOrDefaultAsync();
            if (userRoleId != 0)
            {
                var userIdsInRole = _db.UserRoles.Where(ur => ur.RoleId == userRoleId).Select(ur => ur.UserId);
                query = query.Where(u => userIdsInRole.Contains(u.Id));
            }
            else
            {
                // No users if role is missing
                Users = new List<ApplicationUser>();
                return;
            }

            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim();
                query = query.Where(u => (u.UserName ?? "").Contains(s) || (u.Email ?? "").Contains(s) || (u.FirstName ?? "").Contains(s) || (u.LastName ?? "").Contains(s));
            }

            Users = await query.OrderBy(u => u.Id).ToListAsync();
        }

        public async Task<IActionResult> OnPostSoftDelete(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return RedirectToPage();

            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;
            await _db.SaveChangesAsync();
            return RedirectToPage(new { Search });
        }

        public async Task<IActionResult> OnPostRestore(int id)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return RedirectToPage();

            user.LockoutEnd = null;
            await _db.SaveChangesAsync();
            return RedirectToPage(new { Search });
        }

        public bool IsLocked(ApplicationUser u)
        {
            return u.LockoutEnabled && u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow;
        }
    }
}