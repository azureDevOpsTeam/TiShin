using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using TiShinShop.Data;
using TiShinShop.Entities;
using TiShinShop.Extenssions.Enums;

namespace TiShinShop.Pages.SellerPanel.Settings.Sizes;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CreateModel(ApplicationDbContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> SizeTypes { get; set; } = new();

    public class InputModel
    {
        [Required]
        public SizeType SizeType { get; set; }
        [Required]
        public string Value { get; set; } = string.Empty;
    }

    public void OnGet()
    {
        SizeTypes = new List<SelectListItem>
        {
            new SelectListItem("اندازه عددی", ((int)SizeType.NumericSize).ToString()),
            new SelectListItem("اندازه حرفی", ((int)SizeType.LetterSize).ToString()),
        };
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            OnGet();
            return Page();
        }
        var entity = new Size { SizeType = Input.SizeType, Value = Input.Value };
        _db.Sizes.Add(entity);
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }
}