using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;
using TiShinShop.Extenssions.Enums;

namespace TiShinShop.Pages.SellerPanel.Products;

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

        public SizeType SelectedSizeType { get; set; } = SizeType.LetterSize;
        public List<int> SelectedSizeIds { get; set; } = new();
        public List<int> SelectedColorIds { get; set; } = new();
        public List<int> SelectedMaterialIds { get; set; } = new();
        public int? SelectedCategoryId { get; set; }

        public List<IFormFile> Images { get; set; } = new();
    }

    public async Task<IActionResult> OnGet()
    {
        Colors = await _db.Colors.AsNoTracking().OrderBy(c => c.Value).ToListAsync();
        Materials = await _db.Materials.AsNoTracking().OrderBy(m => m.Name).ToListAsync();
        SizesLetter = await _db.Sizes.AsNoTracking().Where(s => s.SizeType == SizeType.LetterSize).OrderBy(s => s.Value).ToListAsync();
        SizesNumeric = await _db.Sizes.AsNoTracking().Where(s => s.SizeType == SizeType.NumericSize).OrderBy(s => s.Value).ToListAsync();
        LinkableCategories = await _db.Categories.AsNoTracking().Where(c => c.ParentId != 0 && !c.IsBaseMenu).OrderBy(c => c.Name).ToListAsync();

        var product = await _db.Products
            .Include(p => p.Colors)
            .Include(p => p.Materials)
            .Include(p => p.Sizes)
            .Include(p => p.ProductCategories)
            .FirstOrDefaultAsync(p => p.Id == Id);
        if (product == null) return RedirectToPage("Index");

        Input = new InputModel
        {
            Title = product.Title,
            Code = product.Code,
            Brand = product.Brand,
            Description = product.Description,
            BasePrice = product.BasePrice,
            Quantity = product.Quantity,
            SelectedCategoryId = product.ProductCategories?.FirstOrDefault()?.CategoryId,
            SelectedColorIds = product.Colors?.Select(c => c.ColorId).ToList() ?? new List<int>(),
            SelectedMaterialIds = product.Materials?.Select(m => m.MaterialId).ToList() ?? new List<int>(),
            SelectedSizeIds = product.Sizes?.Select(s => s.SizeId).ToList() ?? new List<int>()
        };

        // Guess size type by existing selected sizes
        var anyNumeric = SizesNumeric.Any(s => Input.SelectedSizeIds.Contains(s.Id));
        Input.SelectedSizeType = anyNumeric ? SizeType.NumericSize : SizeType.LetterSize;

        return Page();
    }

    public async Task<IActionResult> OnPost()
    {
        // reload lists for validation errors
        await OnGet();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        var product = await _db.Products
            .Include(p => p.Colors)
            .Include(p => p.Materials)
            .Include(p => p.Sizes)
            .Include(p => p.ProductCategories)
            .FirstOrDefaultAsync(p => p.Id == Id);
        if (product == null) return RedirectToPage("Index");

        product.Title = Input.Title;
        product.Code = Input.Code;
        product.Brand = Input.Brand;
        product.Description = Input.Description;
        product.BasePrice = Input.BasePrice;
        product.Quantity = Input.Quantity;

        // Update relations: clear then add
        var existingColors = _db.ProductColors.Where(pc => pc.ProductId == product.Id);
        _db.ProductColors.RemoveRange(existingColors);
        foreach (var cid in Input.SelectedColorIds.Distinct())
            _db.ProductColors.Add(new ProductColor { ProductId = product.Id, ColorId = cid });

        var existingMaterials = _db.ProductMaterials.Where(pm => pm.ProductId == product.Id);
        _db.ProductMaterials.RemoveRange(existingMaterials);
        foreach (var mid in Input.SelectedMaterialIds.Distinct())
            _db.ProductMaterials.Add(new ProductMaterial { ProductId = product.Id, MaterialId = mid });

        var existingSizes = _db.ProductSizes.Where(ps => ps.ProductId == product.Id);
        _db.ProductSizes.RemoveRange(existingSizes);
        foreach (var sid in Input.SelectedSizeIds.Distinct())
            _db.ProductSizes.Add(new ProductSize { ProductId = product.Id, SizeId = sid });

        var existingCats = _db.ProductCategories.Where(pc => pc.ProductId == product.Id);
        _db.ProductCategories.RemoveRange(existingCats);
        if (Input.SelectedCategoryId.HasValue)
            _db.ProductCategories.Add(new ProductCategory { ProductId = product.Id, CategoryId = Input.SelectedCategoryId.Value });

        // New images
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
        return RedirectToPage("Index");
    }
}