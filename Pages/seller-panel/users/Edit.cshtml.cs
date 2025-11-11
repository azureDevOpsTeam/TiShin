using System;
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
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        public EditModel(ApplicationDbContext db) { _db = db; }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public ApplicationUser? UserItem { get; set; }

        public async Task<IActionResult> OnGet()
        {
            UserItem = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == Id);
            if (UserItem == null) return RedirectToPage("Index");
            return Page();
        }

        public async Task<IActionResult> OnPostLock()
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == Id);
            if (user == null) return RedirectToPage("Index");
            user.LockoutEnabled = true;
            user.LockoutEnd = DateTimeOffset.MaxValue;
            await _db.SaveChangesAsync();
            return RedirectToPage(new { id = Id });
        }

        public async Task<IActionResult> OnPostUnlock()
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == Id);
            if (user == null) return RedirectToPage("Index");
            user.LockoutEnd = null;
            await _db.SaveChangesAsync();
            return RedirectToPage(new { id = Id });
        }

        public bool IsLocked(ApplicationUser u)
        {
            return u.LockoutEnabled && u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.UtcNow;
        }
    }
}