using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.DTOs.Product;

namespace TiShinShop.Pages.Store.Discounts;

public class Over40Model : PageModel
{
    private readonly ApplicationDbContext _db;
    public Over40Model(ApplicationDbContext db) { _db = db; }

    [FromQuery]
    public int page { get; set; } = 1;

    public int pageSize { get; set; } = 20;

    public int totalCount { get; set; }

    public int totalPages => (int)Math.Ceiling((double)totalCount / pageSize);

    public List<ProductListViewModel> Products { get; set; } = new();

    public async Task OnGet()
    {
        var now = DateTime.UtcNow;
        var query = _db.Products
            .Where(p => p.DiscountId != null)
            .Where(p =>
                p.Discount != null &&
                p.Discount.Percentage >= 40 &&
                (!p.Discount.StartDate.HasValue || now >= p.Discount.StartDate.Value) &&
                (!p.Discount.EndDate.HasValue || now <= p.Discount.EndDate.Value));

        totalCount = await query.CountAsync();
        if (page < 1) page = 1;
        var skip = (page - 1) * pageSize;

        Products = await query
            .AsNoTracking()
            .OrderByDescending(p => p.Discount.Percentage)
            .ThenByDescending(p => p.CreateAt)
            .Skip(skip)
            .Take(pageSize)
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
                Discount = p.Discount != null && p.Discount.Percentage >= 40 ? p.Discount.Percentage : (int?)null,
                Colors = p.Colors.Select(c => c.Color.Value).ToArray()
            })
            .ToListAsync();
    }
}
