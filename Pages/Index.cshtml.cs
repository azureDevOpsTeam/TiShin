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

        OutPut.NewProducts = await query.Take(10)
            .OrderByDescending(current => current.CreateAt)
            .Select(current => new ListViewModel
            {
                ProductId = current.Id,
                Title = current.Title,
                Quantity = current.Quantity,
                BasePrice = current.BasePrice,
                FirstImage = current.FirstImage,
                SecondImage = current.SecondImage,
                Colors = current.Colors.Select(c => new ColorDataViewModel { HexCode = c.Color.HexCode, Title = c.Color.Value }).ToArray()
            }).ToListAsync();

        OutPut.TopVisitProducts = await query.Take(10)
            .OrderByDescending(current => current.Visit)
            .Select(current => new ListViewModel
            {
                ProductId = current.Id,
                Title = current.Title,
                Quantity = current.Quantity,
                BasePrice = current.BasePrice,
                FirstImage = current.FirstImage,
                SecondImage = current.SecondImage,
                Colors = current.Colors.Select(c => new ColorDataViewModel { HexCode = c.Color.HexCode, Title = c.Color.Value }).ToArray()
            }).ToListAsync();

        // Approximate best sellers using Visit count (simple proxy)
        OutPut.TopSellingProducts = await query.Take(10)
            .OrderByDescending(current => current.Visit)
            .Select(current => new ListViewModel
            {
                ProductId = current.Id,
                Title = current.Title,
                Quantity = current.Quantity,
                BasePrice = current.BasePrice,
                FirstImage = current.FirstImage,
                SecondImage = current.SecondImage,
                Colors = current.Colors.Select(c => new ColorDataViewModel { HexCode = c.Color.HexCode, Title = c.Color.Value }).ToArray()
            }).ToListAsync();

        OutPut.DiscountProducts = await query.Take(10)
        .OrderByDescending(current => current.DiscountId)
        .Select(current => new ListViewModel
        {
            ProductId = current.Id,
            Title = current.Title,
            Quantity = current.Quantity,
            BasePrice = current.BasePrice,
            FirstImage = current.FirstImage,
            SecondImage = current.SecondImage,
            Colors = current.Colors.Select(c => new ColorDataViewModel { HexCode = c.Color.HexCode, Title = c.Color.Value }).ToArray()
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
                FirstImage = p.FirstImage,
                SecondImage = p.SecondImage,
                Colors = p.Colors.Select(c => new ColorDataViewModel { HexCode = c.Color.HexCode, Title = c.Color.Value }).ToArray()
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
                FirstImage = p.FirstImage,
                SecondImage = p.SecondImage,
                Colors = p.Colors.Select(c => new ColorDataViewModel { HexCode = c.Color.HexCode, Title = c.Color.Value }).ToArray()
            })
            .ToListAsync();
    }
}