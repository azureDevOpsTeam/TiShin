using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Settings.Categories;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public DeleteModel(ApplicationDbContext db) { _db = db; }

    public Category? Item { get; set; }
    public string ParentName { get; set; } = "—";

    public async Task<IActionResult> OnGet(int id)
    {
        Item = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        if (Item == null) return RedirectToPage("Index");
        if (Item.ParentId != 0)
        {
            var parent = await _db.Categories.FindAsync(Item.ParentId);
            ParentName = parent?.Name ?? "نامشخص";
        }
        return Page();
    }

    public async Task<IActionResult> OnPost(int id)
    {
        var item = await _db.Categories.FindAsync(id);
        if (item != null)
        {
            _db.Categories.Remove(item);
            await _db.SaveChangesAsync();
        }
        return RedirectToPage("Index");
    }
}