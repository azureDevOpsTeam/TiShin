using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;
using VitrineEntity = TiShinShop.Entities.Vitrine;

namespace TiShinShop.Pages.SellerPanel.Vitrine;

[Authorize(Roles = "Seller")]
public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public IndexModel(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    public IList<VitrineEntity> Items { get; set; } = new List<VitrineEntity>();

    public async Task OnGet()
    {
        Items = await _db.Vitrines.AsNoTracking().OrderByDescending(v => v.CreateAt).Take(100).ToListAsync();
    }

    public async Task<IActionResult> OnPostDelete(int id)
    {
        var item = await _db.Vitrines.FindAsync(id);
        if (item != null)
        {
            if (!string.IsNullOrWhiteSpace(item.ImageUrl))
            {
                var path = item.ImageUrl.StartsWith("~") ? item.ImageUrl.TrimStart('~').TrimStart('/') : item.ImageUrl.TrimStart('/');
                var fullPath = Path.Combine(_env.WebRootPath, path.Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath))
                {
                    try { System.IO.File.Delete(fullPath); } catch { }
                }
            }
            _db.Vitrines.Remove(item);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("/seller-panel/vitrine/Index");
    }
}
