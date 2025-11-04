using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Settings.Materials;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) { _db = db; }

    public IList<Material> Items { get; set; } = new List<Material>();

    public async Task OnGet()
    {
        Items = await _db.Materials.AsNoTracking().OrderBy(m => m.Name).ToListAsync();
    }
}