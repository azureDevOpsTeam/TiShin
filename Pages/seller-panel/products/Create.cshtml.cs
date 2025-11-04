using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;
using TiShinShop.Extenssions.Enums;

namespace TiShinShop.Pages.SellerPanel.Products;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    public CreateModel(ApplicationDbContext db, IWebHostEnvironment env) { _db = db; _env = env; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public IList<Color> Colors { get; set; } = new List<Color>();
    public IList<Material> Materials { get; set; } = new List<Material>();
    public IList<Size> SizesLetter { get; set; } = new List<Size>();
    public IList<Size> SizesNumeric { get; set; } = new List<Size>();
    public IList<Category> LinkableCategories { get; set; } = new List<Category>();

    public class InputModel
    {
        [Required]
        public string Title { get; set; } = string.Empty;
        [Required]
        public string Code { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [Required]
        public decimal BasePrice { get; set; }
        [Required]
        public int Quantity { get; set; }

        public decimal? ShippingCost { get; set; }

        public SizeType SelectedSizeType { get; set; } = SizeType.LetterSize;
        public List<int> SelectedSizeIds { get; set; } = new();
        public List<int> SelectedColorIds { get; set; } = new();
        public List<int> SelectedMaterialIds { get; set; } = new();
        [Required]
        public int? SelectedCategoryId { get; set; }

        public List<IFormFile> Images { get; set; } = new();
    }

    public async Task OnGet()
    {
        Colors = await _db.Colors.AsNoTracking().OrderBy(c => c.Value).ToListAsync();
        Materials = await _db.Materials.AsNoTracking().OrderBy(m => m.Name).ToListAsync();
        SizesLetter = await _db.Sizes.AsNoTracking().Where(s => s.SizeType == SizeType.LetterSize).OrderBy(s => s.Value).ToListAsync();
        SizesNumeric = await _db.Sizes.AsNoTracking().Where(s => s.SizeType == SizeType.NumericSize).OrderBy(s => s.Value).ToListAsync();
        LinkableCategories = await _db.Categories.AsNoTracking().Where(c => c.ParentId != 0 && !c.IsBaseMenu).OrderBy(c => c.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPost()
    {
        // Reload lists if validation fails
        await OnGet();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var product = new Product
        {
            Title = Input.Title,
            Code = Input.Code,
            Brand = Input.Brand,
            Description = Input.Description,
            BasePrice = Input.BasePrice,
            Quantity = Input.Quantity,
            Visit = 0,
        };

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        if (Input.SelectedColorIds?.Count > 0)
        {
            foreach (var cid in Input.SelectedColorIds.Distinct())
            {
                _db.ProductColors.Add(new ProductColor { ProductId = product.Id, ColorId = cid });
            }
        }

        if (Input.SelectedMaterialIds?.Count > 0)
        {
            foreach (var mid in Input.SelectedMaterialIds.Distinct())
            {
                _db.ProductMaterials.Add(new ProductMaterial { ProductId = product.Id, MaterialId = mid });
            }
        }

        if (Input.SelectedSizeIds?.Count > 0)
        {
            foreach (var sid in Input.SelectedSizeIds.Distinct())
            {
                _db.ProductSizes.Add(new ProductSize { ProductId = product.Id, SizeId = sid });
            }
        }

        if (Input.SelectedCategoryId.HasValue)
        {
            _db.ProductCategories.Add(new ProductCategory { ProductId = product.Id, CategoryId = Input.SelectedCategoryId.Value });
        }

        // Save uploaded images
        if (Input.Images != null && Input.Images.Count > 0)
        {
            var productImagesDir = Path.Combine(_env.WebRootPath, "images", "product");
            Directory.CreateDirectory(productImagesDir);

            foreach (var file in Input.Images.Where(f => f != null && f.Length > 0))
            {
                var ext = Path.GetExtension(file.FileName);
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext.ToLower())) continue;

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var savePath = Path.Combine(productImagesDir, fileName);
                using (var stream = System.IO.File.Create(savePath))
                {
                    await file.CopyToAsync(stream);
                }

                var url = $"/images/product/{fileName}";
                _db.ProductImages.Add(new ProductImage { ProductId = product.Id, ImageUrl = url });
            }
        }

        await _db.SaveChangesAsync();

        return RedirectToPage("/seller-panel/Index");
    }
}