using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Settings.Colors;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DeleteModel(ApplicationDbContext db) { _db = db; }

    public Color Item { get; set; }

    public async Task<IActionResult> OnGet(int id)
    {
        Item = await _db.Colors.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (Item == null) return RedirectToPage("Index");
        return Page();
    }

    public async Task<IActionResult> OnPost(int id)
    {
        var item = await _db.Colors.FindAsync(id);
        if (item != null)
        {
            _db.Colors.Remove(item);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}