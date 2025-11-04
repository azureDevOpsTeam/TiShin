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
    }
}