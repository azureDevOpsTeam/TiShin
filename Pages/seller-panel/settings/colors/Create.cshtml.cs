using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Settings.Colors;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        [Required]
        [Display(Name = "نام رنگ")]
        public string Value { get; set; } = string.Empty;

        [Required]
        [Display(Name = "کد هگز")]
        [RegularExpression("^#(?:[0-9a-fA-F]{3}){1,2}$", ErrorMessage = "کد هگز معتبر وارد کنید.")]
        public string HexCode { get; set; } = "#000000";
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid) return Page();

        var color = new Color { Value = Input.Value, HexCode = Input.HexCode };
        _db.Colors.Add(color);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}