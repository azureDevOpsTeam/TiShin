using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.DTOs.Product;

namespace TiShinShop.Pages.Categories;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public IndexModel(ApplicationDbContext db) { _db = db; }

    [FromRoute]
    public int? Id { get; set; }

    public List<ProductListViewModel> Products { get; set; } = new();

    public string? CategoryTitle { get; set; }

    public async Task OnGet(int? id)
    {
        Id = id;

        IQueryable<TiShinShop.Entities.Product> query = _db.Products;

        if (Id.HasValue)
        {
            var cat = await _db.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == Id.Value);
            CategoryTitle = cat?.Name;
            ViewData["Title"] = CategoryTitle ?? "دسته‌بندی";

            var productIds = await _db.ProductCategories
                .Where(pc => pc.CategoryId == Id.Value)
                .Select(pc => pc.ProductId)
                .Distinct()
                .ToListAsync();

            query = query.Where(p => productIds.Contains(p.Id));
        }
        else
        {
            ViewData["Title"] = "دسته‌بندی";
        }

        Products = await query
            .AsNoTracking()
            .OrderByDescending(p => p.CreateAt)
            .Select(p => new ProductListViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Code = p.Code,
                Brand = p.Brand,
                Description = p.Description,
                FirstImage = p.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? p.FirstImage,
                SecondImage = p.Images.Select(i => i.ImageUrl).Skip(1).FirstOrDefault() ?? p.SecondImage,
                BasePrice = p.BasePrice,
                Quantity = p.Quantity,
                Discount = p.Discount != null && p.Discount.IsActive ? p.Discount.Percentage : (int?)null,
                Colors = p.Colors.Select(c => c.Color.Value).ToArray()
            })
            .ToListAsync();
    }
}