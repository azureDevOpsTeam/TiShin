using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Settings.Categories;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IList<Category> AllCategories { get; set; } = new List<Category>();

    public class InputModel
    {
        [Required]
        public string Name { get; set; } = string.Empty;
        public int ParentId { get; set; } = 0;
        public bool IsBaseMenu { get; set; }
    }

    public async Task OnGet()
    {
        AllCategories = await _db.Categories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPost()
    {
        await OnGet();
        if (!ModelState.IsValid) return Page();
        var cat = new Category { Name = Input.Name, ParentId = Input.ParentId, IsBaseMenu = Input.IsBaseMenu };
        _db.Categories.Add(cat);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}