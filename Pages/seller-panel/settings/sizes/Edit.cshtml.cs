using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;
using TiShinShop.Extenssions.Enums;

namespace TiShinShop.Pages.SellerPanel.Settings.Sizes;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public EditModel(ApplicationDbContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<SelectListItem> SizeTypes { get; set; } = new();

    public class InputModel
    {
        public int Id { get; set; }
        [Required]
        public SizeType SizeType { get; set; }
        [Required]
        public string Value { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGet(int id)
    {
        var item = await _db.Sizes.FindAsync(id);
        if (item == null) return RedirectToPage("Index");
        Input = new InputModel { Id = item.Id, SizeType = item.SizeType, Value = item.Value };
        LoadSizeTypes();
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        LoadSizeTypes();
        if (!ModelState.IsValid) return Page();
        var item = await _db.Sizes.FirstOrDefaultAsync(s => s.Id == Input.Id);
        if (item == null) return RedirectToPage("Index");
        item.SizeType = Input.SizeType;
        item.Value = Input.Value;
        await _db.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    private void LoadSizeTypes()
    {
        SizeTypes = new List<SelectListItem>
        {
            new SelectListItem("اندازه عددی", ((int)SizeType.NumericSize).ToString()),
            new SelectListItem("اندازه حرفی", ((int)SizeType.LetterSize).ToString()),
        };
    }
}