using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Settings.Materials;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGet(int id)
    {
        var item = await _db.Materials.FindAsync(id);
        if (item == null) return RedirectToPage("Index");
        Input = new InputModel { Id = item.Id, Name = item.Name };
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid) return Page();
        var item = await _db.Materials.FirstOrDefaultAsync(m => m.Id == Input.Id);
        if (item == null) return RedirectToPage("Index");
        item.Name = Input.Name;
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}