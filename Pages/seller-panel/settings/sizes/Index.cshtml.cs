using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Settings.Sizes;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) { _db = db; }

    public IList<Size> Items { get; set; } = new List<Size>();

    public async Task OnGet()
    {
        Items = await _db.Sizes.AsNoTracking().OrderBy(s => s.SizeType).ThenBy(s => s.Value).ToListAsync();
    }
}