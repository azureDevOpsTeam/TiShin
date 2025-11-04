using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Settings.Colors;

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
        public string Value { get; set; } = string.Empty;

        [Required]
        [RegularExpression("^#(?:[0-9a-fA-F]{3}){1,2}$", ErrorMessage = "کد هگز معتبر وارد کنید.")]
        public string HexCode { get; set; } = "#000000";
    }

    public async Task<IActionResult> OnGet(int id)
    {
        var item = await _db.Colors.FindAsync(id);
        if (item == null) return RedirectToPage("Index");
        Input = new InputModel { Id = item.Id, Value = item.Value, HexCode = item.HexCode };
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid) return Page();
        var item = await _db.Colors.FirstOrDefaultAsync(c => c.Id == Input.Id);
        if (item == null) return RedirectToPage("Index");
        item.Value = Input.Value;
        item.HexCode = Input.HexCode;
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}