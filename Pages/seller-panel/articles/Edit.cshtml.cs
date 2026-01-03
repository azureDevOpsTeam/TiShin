using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Articles;

[Authorize(Roles = "Seller")]
public class EditModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public EditModel(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string ContentHtml { get; set; } = string.Empty;
        public IFormFile? Image { get; set; }
        public bool IsPublished { get; set; } = true;
    }

    public async Task<IActionResult> OnGet()
    {
        var a = await _db.Articles.FirstOrDefaultAsync(x => x.Id == Id);
        if (a == null) return RedirectToPage("Index");
        Input = new InputModel
        {
            Title = a.Title,
            Summary = a.Summary,
            ContentHtml = a.ContentHtml,
            IsPublished = a.IsPublished
        };
        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        var a = await _db.Articles.FirstOrDefaultAsync(x => x.Id == Id);
        if (a == null) return RedirectToPage("Index");
        a.Title = Input.Title;
        a.Summary = Input.Summary;
        a.ContentHtml = Input.ContentHtml;
        a.IsPublished = Input.IsPublished;
        a.UpdatedAt = DateTime.UtcNow;
        if (Input.Image != null && Input.Image.Length > 0 && Input.Image.ContentType.StartsWith("image/"))
        {
            var dir = Path.Combine(_env.WebRootPath, "images", "blog");
            Directory.CreateDirectory(dir);
            var ext = Path.GetExtension(Input.Image.FileName);
            var name = $"{Guid.NewGuid():N}{ext}";
            var full = Path.Combine(dir, name);
            using (var stream = System.IO.File.Create(full))
            {
                await Input.Image.CopyToAsync(stream);
            }
            a.ImageUrl = $"/images/blog/{name}";
        }
        await _db.SaveChangesAsync();
        return RedirectToPage("/seller-panel/articles/Index");
    }
}
