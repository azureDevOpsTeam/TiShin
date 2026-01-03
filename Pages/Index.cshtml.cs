using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.DTOs.Product;

namespace TiShinShop.Pages;

public class IndexModel(ApplicationDbContext context) : PageModel
{
    private readonly ApplicationDbContext _context = context;

    [BindProperty]
    public ProductHomeViewModel OutPut { get; set; }

    public async Task OnGet()
    {
        OutPut = new();
        var query = await Task.Run(() => _context.Products.AsQueryable());

        OutPut.NewProducts = await query
            .OrderByDescending(current => current.CreateAt)
            .Take(10)
            .Select(current => new ListViewModel
            {
                ProductId = current.Id,
                Title = current.Title,
                Quantity = current.Quantity,
                BasePrice = current.BasePrice,
                FirstImage = current.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? current.FirstImage,
                SecondImage = current.Images.Select(i => i.ImageUrl).Skip(1).FirstOrDefault() ?? current.SecondImage,
                Colors = current.Colors.Select(c => new ColorDataViewModel { HexCode = c.Color.HexCode, Title = c.Color.Value }).ToArray(),
                DiscountPercent = current.Discount != null && current.Discount.IsActive ? current.Discount.Percentage : (int?)null
            }).ToListAsync();

        OutPut.TopVisitProducts = await query
            .OrderByDescending(current => current.Visit)
            .Take(10)
            .Select(current => new ListViewModel
            {
                ProductId = current.Id,
                Title = current.Title,
                Quantity = current.Quantity,
                BasePrice = current.BasePrice,
                FirstImage = current.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? current.FirstImage,
                SecondImage = current.Images.Select(i => i.ImageUrl).Skip(1).FirstOrDefault() ?? current.SecondImage,
                Colors = current.Colors.Select(c => new ColorDataViewModel { HexCode = c.Color.HexCode, Title = c.Color.Value }).ToArray(),
                DiscountPercent = current.Discount != null && current.Discount.IsActive ? current.Discount.Percentage : (int?)null
            }).ToListAsync();

        // Approximate best sellers using Visit count (simple proxy)
        OutPut.TopSellingProducts = await query
            .OrderByDescending(current => current.Visit)
            .Take(10)
            .Select(current => new ListViewModel
            {
                ProductId = current.Id,
                Title = current.Title,
                Quantity = current.Quantity,
                BasePrice = current.BasePrice,
                FirstImage = current.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? current.FirstImage,
                SecondImage = current.Images.Select(i => i.ImageUrl).Skip(1).FirstOrDefault() ?? current.SecondImage,
                Colors = current.Colors.Select(c => new ColorDataViewModel { HexCode = c.Color.HexCode, Title = c.Color.Value }).ToArray(),
                DiscountPercent = current.Discount != null && current.Discount.IsActive ? current.Discount.Percentage : (int?)null
            }).ToListAsync();

        OutPut.DiscountProducts = await query
        .OrderByDescending(current => current.DiscountId)
        .Take(10)
        .Select(current => new ListViewModel
        {
            ProductId = current.Id,
            Title = current.Title,
            Quantity = current.Quantity,
            BasePrice = current.BasePrice,
            FirstImage = current.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? current.FirstImage,
            SecondImage = current.Images.Select(i => i.ImageUrl).Skip(1).FirstOrDefault() ?? current.SecondImage,
            Colors = current.Colors.Select(c => new ColorDataViewModel { HexCode = c.Color.HexCode, Title = c.Color.Value }).ToArray(),
            DiscountPercent = current.Discount != null && current.Discount.IsActive ? current.Discount.Percentage : (int?)null
        }).ToListAsync();

        // Latest Laptops by category name
        var laptopCategoryIds = await _context.Categories
            .Where(c => EF.Functions.Like(c.Name, "%لپتاپ%") || EF.Functions.Like(c.Name, "%لپ تاپ%"))
            .Select(c => c.Id)
            .ToListAsync();

        var laptopProductIds = await _context.ProductCategories
            .Where(pc => laptopCategoryIds.Contains(pc.CategoryId))
            .Select(pc => pc.ProductId)
            .Distinct()
            .ToListAsync();

        OutPut.LatestLaptops = await query
            .Where(p => laptopProductIds.Contains(p.Id))
            .OrderByDescending(p => p.CreateAt)
            .Take(10)
            .Select(p => new ListViewModel
            {
                ProductId = p.Id,
                Title = p.Title,
                Quantity = p.Quantity,
                BasePrice = p.BasePrice,
                FirstImage = p.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? p.FirstImage,
                SecondImage = p.Images.Select(i => i.ImageUrl).Skip(1).FirstOrDefault() ?? p.SecondImage,
                Colors = p.Colors.Select(c => new ColorDataViewModel { HexCode = c.Color.HexCode, Title = c.Color.Value }).ToArray(),
                DiscountPercent = p.Discount != null && p.Discount.IsActive ? p.Discount.Percentage : (int?)null
            })
            .ToListAsync();

        // Latest Clothes by category name
        var clothesCategoryIds = await _context.Categories
            .Where(c => EF.Functions.Like(c.Name, "%لباس%"))
            .Select(c => c.Id)
            .ToListAsync();

        var clothesProductIds = await _context.ProductCategories
            .Where(pc => clothesCategoryIds.Contains(pc.CategoryId))
            .Select(pc => pc.ProductId)
            .Distinct()
            .ToListAsync();

        OutPut.LatestClothes = await query
            .Where(p => clothesProductIds.Contains(p.Id))
            .OrderByDescending(p => p.CreateAt)
            .Take(10)
            .Select(p => new ListViewModel
            {
                ProductId = p.Id,
                Title = p.Title,
                Quantity = p.Quantity,
                BasePrice = p.BasePrice,
                FirstImage = p.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? p.FirstImage,
                SecondImage = p.Images.Select(i => i.ImageUrl).Skip(1).FirstOrDefault() ?? p.SecondImage,
                Colors = p.Colors.Select(c => new ColorDataViewModel { HexCode = c.Color.HexCode, Title = c.Color.Value }).ToArray(),
                DiscountPercent = p.Discount != null && p.Discount.IsActive ? p.Discount.Percentage : (int?)null
            })
            .ToListAsync();

        // Fallback: اگر دسته «لباس» خالی بود، جدیدترین محصولات را نشان بده
        if (OutPut.LatestClothes == null || OutPut.LatestClothes.Count == 0)
        {
            OutPut.LatestClothes = OutPut.NewProducts;
        }

        OutPut.VitrineImages = await _context.Vitrines
            .AsNoTracking()
            .OrderByDescending(v => v.CreateAt)
            .Select(v => v.ImageUrl)
            .Take(4)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var discountQuery = _context.Products
            .Where(p => p.DiscountId != null)
            .Where(p =>
                p.Discount != null &&
                p.Discount.Percentage >= 40 &&
                (!p.Discount.StartDate.HasValue || now >= p.Discount.StartDate.Value) &&
                (!p.Discount.EndDate.HasValue || now <= p.Discount.EndDate.Value));

        OutPut.SpecialDiscountCount = await discountQuery.CountAsync();
        OutPut.SpecialDiscountProducts = await discountQuery
            .OrderByDescending(p => p.Discount.Percentage)
            .ThenByDescending(p => p.CreateAt)
            .Take(5)
            .Select(p => new ListViewModel
            {
                ProductId = p.Id,
                Title = p.Title,
                Quantity = p.Quantity,
                BasePrice = p.BasePrice,
                FirstImage = p.Images.Select(i => i.ImageUrl).FirstOrDefault() ?? p.FirstImage,
                SecondImage = p.Images.Select(i => i.ImageUrl).Skip(1).FirstOrDefault() ?? p.SecondImage,
                Colors = p.Colors.Select(c => new ColorDataViewModel { HexCode = c.Color.HexCode, Title = c.Color.Value }).ToArray()
            })
            .ToListAsync();

        OutPut.LatestArticles = await _context.Articles
            .AsNoTracking()
            .Where(a => a.IsPublished)
            .OrderByDescending(a => a.CreatedAt)
            .Take(10)
            .Select(a => new ArticleListViewModel
            {
                Id = a.Id,
                Title = a.Title,
                Slug = a.Slug,
                ImageUrl = a.ImageUrl,
                CreatedAt = a.CreatedAt,
                Summary = a.Summary,
                Views = a.Views
            })
            .ToListAsync();
    }
}
