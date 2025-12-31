using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiShinShop.Data;
using TiShinShop.Entities;
using VitrineEntity = TiShinShop.Entities.Vitrine;

namespace TiShinShop.Pages.SellerPanel.Vitrine;

[Authorize(Roles = "Seller")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public CreateModel(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    [BindProperty]
    public IFormFile Image { get; set; }

    public void OnGet() { }

    public async Task<IActionResult> OnPost()
    {
        if (Image == null || Image.Length == 0 || !Image.ContentType.StartsWith("image/"))
        {
            ModelState.AddModelError(string.Empty, "انتخاب تصویر معتبر الزامی است");
            return Page();
        }
        var folder = Path.Combine(_env.WebRootPath, "images", "vitrine");
        Directory.CreateDirectory(folder);
        var ext = Path.GetExtension(Image.FileName);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var path = Path.Combine(folder, fileName);
        using (var stream = new FileStream(path, FileMode.Create))
        {
            await Image.CopyToAsync(stream);
        }
        var relative = $"/images/vitrine/{fileName}";
        var v = new VitrineEntity { ImageUrl = relative };
        _db.Vitrines.Add(v);
        await _db.SaveChangesAsync();
        return RedirectToPage("/seller-panel/vitrine/Index");
    }
}
