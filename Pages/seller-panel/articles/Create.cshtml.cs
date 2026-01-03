using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.SellerPanel.Articles;

[Authorize(Roles = "Seller")]
public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public CreateModel(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

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

    public void OnGet() { }

    public async Task<IActionResult> OnPost()
    {
        if (string.IsNullOrWhiteSpace(Input.Title))
        {
            ModelState.AddModelError(nameof(Input.Title), "عنوان الزامی است");
            return Page();
        }
        string? imageUrl = null;
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
            imageUrl = $"/images/blog/{name}";
        }
        var slug = GenerateSlug(Input.Title);
        var article = new Article
        {
            Title = Input.Title,
            Summary = Input.Summary,
            ContentHtml = Input.ContentHtml,
            ImageUrl = imageUrl ?? string.Empty,
            Slug = slug,
            IsPublished = Input.IsPublished,
            CreatedAt = DateTime.UtcNow
        };
        _db.Articles.Add(article);
        await _db.SaveChangesAsync();
        return RedirectToPage("/seller-panel/articles/Index");
    }

    private static string GenerateSlug(string input)
    {
        var s = input.Trim().ToLowerInvariant();
        s = string.Join("-", s.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        return s;
    }
}
