using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Settings.Categories;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IList<Category> AllCategories { get; set; } = new List<Category>();

    public class InputModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public int ParentId { get; set; }
        public bool IsBaseMenu { get; set; }
    }

    public async Task<IActionResult> OnGet(int id)
    {
        var cat = await _db.Categories.FindAsync(id);
        if (cat == null) return RedirectToPage("Index");
        Input = new InputModel { Id = cat.Id, Name = cat.Name, ParentId = cat.ParentId, IsBaseMenu = cat.IsBaseMenu };
        AllCategories = await _db.Categories.AsNoTracking().Where(c => c.Id != id).OrderBy(c => c.Name).ToListAsync();
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        AllCategories = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
        if (!ModelState.IsValid) return Page();
        var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == Input.Id);
        if (cat == null) return RedirectToPage("Index");
        cat.Name = Input.Name;
        cat.ParentId = Input.ParentId;
        cat.IsBaseMenu = Input.IsBaseMenu;
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}