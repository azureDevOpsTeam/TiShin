using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Articles;

[Authorize(Roles = "Seller")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) { _db = db; }

    public List<Article> Articles { get; set; } = new();

    public async Task OnGet()
    {
        Articles = await _db.Articles
            .AsNoTracking()
            .OrderByDescending(a => a.CreatedAt)
            .Take(100)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDelete(int id)
    {
        var a = await _db.Articles.FindAsync(id);
        if (a != null)
        {
            _db.Articles.Remove(a);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("/seller-panel/articles/Index");
    }
}
