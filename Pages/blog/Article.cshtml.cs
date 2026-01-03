using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.Blog;

public class ArticleModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public ArticleModel(ApplicationDbContext db) { _db = db; }

    [FromRoute]
    public string Slug { get; set; }

    public Article? Item { get; set; }

    public async Task<IActionResult> OnGet(string slug)
    {
        Slug = slug;
        Item = await _db.Articles.AsNoTracking().FirstOrDefaultAsync(a => a.Slug == slug && a.IsPublished);
        if (Item == null) return NotFound();
        return Page();
    }
}
