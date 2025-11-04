using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Settings.Colors;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) { _db = db; }

    public IList<Color> Items { get; set; } = new List<Color>();

    public async Task OnGet()
    {
        Items = await _db.Colors.AsNoTracking().OrderBy(c => c.Value).ToListAsync();
    }
}