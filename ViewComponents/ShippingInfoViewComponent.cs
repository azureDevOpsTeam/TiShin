using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using TiShinShop.Data;
using TiShinShop.DTOs.Product;

namespace TiShin.ViewComponents;

public class ShippingInfoViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public ShippingInfoViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var username = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(username))
            return View("Empty");

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == username);

        var address = await _context.UserAddresses
            .Where(a => a.UserId == user.Id && a.IsDefault)
            .FirstOrDefaultAsync();

        var cart = await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Discount)
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductColor)
                    .ThenInclude(pc => pc.Color)
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductSize)
                    .ThenInclude(ps => ps.Size)
            .FirstOrDefaultAsync(c => c.UserId == user.Id);

        var shippingCost = 35000m;

        var totalPrice = cart.Items.Sum(i => i.Product.BasePrice * i.Quantity);
        var discountAmount = cart.Items
            .Where(i => i.Product.Discount != null && i.Product.Discount.IsActive)
            .Sum(i => ((i.Product.BasePrice * i.Product.Discount.Percentage) / 100) * i.Quantity);

        var model = new ShippingInfoViewModel
        {
            FullName = $"{user.FirstName} {user.LastName}",
            PhoneNumber = user.PhoneNumber,
            Address = address?.FullAddress ?? "آدرس ثبت نشده",
            PostalCode = address?.PostalCode ?? "-",

            DeliveryMethods = new List<DeliveryMethodDto>
            {
                new DeliveryMethodDto { Name = "ارسال عادی", Type = "normal", Cost = shippingCost },
                new DeliveryMethodDto { Name = "دریافت حضوری", Type = "pickup", Cost = 0 }
            },

            CartItems = cart.Items.Select(i => new CartItemDto
            {
                Title = i.Product.Title,
                Image = i.Product.FirstImage,
                Color = i.ProductColor?.Color?.Value ?? "-",
                Size = i.ProductSize?.Size?.Value ?? "-",
                Quantity = i.Quantity
            }).ToList(),

            ShippingCost = shippingCost,
            TotalPrice = totalPrice,
            DiscountAmount = discountAmount,

            AvailableDays = Enumerable.Range(0, 5).Select(d => new DeliveryDayDto
            {
                Day = DateTime.Today.AddDays(d).ToString("dddd", new CultureInfo("fa-IR")),
                Date = DateTime.Today.AddDays(d).ToString("yyyy/MM/dd")
            }).ToList(),

            TimeSlots = new List<DeliveryTimeSlotDto>
            {
                new() { From = "9", To = "12" },
                new() { From = "12", To = "15" },
                new() { From = "15", To = "18" }
            }
        };

        return View("Default", model);
    }
}