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
    private async Task LoadLookups()
    {
        Colors = await _db.Colors.AsNoTracking().OrderBy(c => c.Value).ToListAsync();
        Materials = await _db.Materials.AsNoTracking().OrderBy(m => m.Name).ToListAsync();
        SizesLetter = await _db.Sizes.AsNoTracking().Where(s => s.SizeType == SizeType.LetterSize).OrderBy(s => s.Value).ToListAsync();
        SizesNumeric = await _db.Sizes.AsNoTracking().Where(s => s.SizeType == SizeType.NumericSize).OrderBy(s => s.Value).ToListAsync();
        LinkableCategories = await _db.Categories.AsNoTracking().Where(c => c.ParentId != 0 && !c.IsBaseMenu).OrderBy(c => c.Name).ToListAsync();
    }

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

        [Range(0, 100)]
        public int? DiscountPercent { get; set; }

        public SizeType SelectedSizeType { get; set; } = SizeType.LetterSize;
        public List<int> SelectedSizeIds { get; set; } = new();
        public List<int> SelectedColorIds { get; set; } = new();
        public List<int> SelectedMaterialIds { get; set; } = new();
        public int? SelectedCategoryId { get; set; }

        public List<IFormFile> Images { get; set; } = new();
    }

    public async Task<IActionResult> OnGet()
    {
        await LoadLookups();

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
            DiscountPercent = (product.Discount != null && product.Discount.IsActive) ? product.Discount.Percentage : (int?)null,
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
        await LoadLookups();

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

        // Update relations safely: only remove unused and unreferenced joins, then add missing
        var existingColorsList = await _db.ProductColors.Where(pc => pc.ProductId == product.Id).ToListAsync();
        var existingColorIds = existingColorsList.Select(pc => pc.ColorId).ToHashSet();
        var referencedProductColorIds = await _db.OrderItems
            .Where(oi => existingColorsList.Select(pc => pc.Id).Contains(oi.ProductColorId))
            .Select(oi => oi.ProductColorId)
            .Distinct()
            .ToListAsync();
        var referencedProductColorIdsSet = referencedProductColorIds.ToHashSet();
        var toDeleteColors = existingColorsList
            .Where(pc => !referencedProductColorIdsSet.Contains(pc.Id) && !Input.SelectedColorIds.Contains(pc.ColorId))
            .ToList();
        if (toDeleteColors.Count > 0)
            _db.ProductColors.RemoveRange(toDeleteColors);
        var toAddColors = Input.SelectedColorIds.Distinct().Where(cid => !existingColorIds.Contains(cid)).ToList();
        foreach (var cid in toAddColors)
            _db.ProductColors.Add(new ProductColor { ProductId = product.Id, ColorId = cid });

        var existingMaterialsList = await _db.ProductMaterials.Where(pm => pm.ProductId == product.Id).ToListAsync();
        var existingMaterialIds = existingMaterialsList.Select(pm => pm.MaterialId).ToHashSet();
        var referencedProductMaterialIds = await _db.OrderItems
            .Where(oi => existingMaterialsList.Select(pm => pm.Id).Contains(oi.ProductMaterialId))
            .Select(oi => oi.ProductMaterialId)
            .Distinct()
            .ToListAsync();
        var referencedProductMaterialIdsSet = referencedProductMaterialIds.ToHashSet();
        var toDeleteMaterials = existingMaterialsList
            .Where(pm => !referencedProductMaterialIdsSet.Contains(pm.Id) && !Input.SelectedMaterialIds.Contains(pm.MaterialId))
            .ToList();
        if (toDeleteMaterials.Count > 0)
            _db.ProductMaterials.RemoveRange(toDeleteMaterials);
        var toAddMaterials = Input.SelectedMaterialIds.Distinct().Where(mid => !existingMaterialIds.Contains(mid)).ToList();
        foreach (var mid in toAddMaterials)
            _db.ProductMaterials.Add(new ProductMaterial { ProductId = product.Id, MaterialId = mid });

        var existingSizesList = await _db.ProductSizes.Where(ps => ps.ProductId == product.Id).ToListAsync();
        var existingSizeIds = existingSizesList.Select(ps => ps.SizeId).ToHashSet();
        var referencedProductSizeIds = await _db.OrderItems
            .Where(oi => existingSizesList.Select(ps => ps.Id).Contains(oi.ProductSizeId))
            .Select(oi => oi.ProductSizeId)
            .Distinct()
            .ToListAsync();
        var referencedProductSizeIdsSet = referencedProductSizeIds.ToHashSet();
        var toDeleteSizes = existingSizesList
            .Where(ps => !referencedProductSizeIdsSet.Contains(ps.Id) && !Input.SelectedSizeIds.Contains(ps.SizeId))
            .ToList();
        if (toDeleteSizes.Count > 0)
            _db.ProductSizes.RemoveRange(toDeleteSizes);
        var toAddSizes = Input.SelectedSizeIds.Distinct().Where(sid => !existingSizeIds.Contains(sid)).ToList();
        foreach (var sid in toAddSizes)
            _db.ProductSizes.Add(new ProductSize { ProductId = product.Id, SizeId = sid });

        var existingCats = _db.ProductCategories.Where(pc => pc.ProductId == product.Id);
        _db.ProductCategories.RemoveRange(existingCats);
        if (Input.SelectedCategoryId.HasValue)
            _db.ProductCategories.Add(new ProductCategory { ProductId = product.Id, CategoryId = Input.SelectedCategoryId.Value });

        if (Input.DiscountPercent.HasValue && Input.DiscountPercent.Value > 0)
        {
            if (product.DiscountId.HasValue)
            {
                var existingDiscount = await _db.Discounts.FirstOrDefaultAsync(d => d.Id == product.DiscountId.Value);
                if (existingDiscount != null)
                {
                    existingDiscount.Percentage = Input.DiscountPercent.Value;
                }
                else
                {
                    var discount = new Discount { Percentage = Input.DiscountPercent.Value };
                    _db.Discounts.Add(discount);
                    await _db.SaveChangesAsync();
                    product.DiscountId = discount.Id;
                }
            }
            else
            {
                var discount = new Discount { Percentage = Input.DiscountPercent.Value };
                _db.Discounts.Add(discount);
                await _db.SaveChangesAsync();
                product.DiscountId = discount.Id;
            }
        }
        else
        {
            product.DiscountId = null;
        }

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
