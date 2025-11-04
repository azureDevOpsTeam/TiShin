using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Settings.Materials;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DeleteModel(ApplicationDbContext db) { _db = db; }

    public Material? Item { get; set; }

    public async Task<IActionResult> OnGet(int id)
    {
        Item = await _db.Materials.AsNoTracking().FirstOrDefaultAsync(m => m.Id == id);
        if (Item == null) return RedirectToPage("Index");
        return Page();
    }

    public async Task<IActionResult> OnPost(int id)
    {
        var item = await _db.Materials.FindAsync(id);
        if (item != null)
        {
            _db.Materials.Remove(item);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}