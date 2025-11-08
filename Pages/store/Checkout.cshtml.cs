using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.Pages.Store;

public class CheckoutModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public CheckoutModel(ApplicationDbContext db) { _db = db; }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public List<UserAddress> Addresses { get; set; } = new();

    [BindProperty]
    public int? SelectedAddressId { get; set; }

    [BindProperty]
    public bool UseNewAddress { get; set; }

    public class InputModel
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required] public string Phone { get; set; } = string.Empty;
        [Required] public string City { get; set; } = string.Empty;
        [Required] public string Street { get; set; } = string.Empty;
        public string FullAddress { get; set; }
        public string PostalCode { get; set; }
    }

    public async Task OnGet()
    {
        var username = User?.Identity?.Name;
        var user = !string.IsNullOrWhiteSpace(username)
            ? await _db.Users.FirstOrDefaultAsync(u => u.UserName == username)
            : null;
        if (user != null)
        {
            Addresses = await _db.UserAddresses
                .Where(a => a.UserId == user.Id)
                .OrderByDescending(a => a.IsDefault)
                .ToListAsync();
        }
    }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid) return Page();
        var username = User?.Identity?.Name;
        var user = !string.IsNullOrWhiteSpace(username)
            ? await _db.Users.FirstOrDefaultAsync(u => u.UserName == username)
            : null;
        var userId = user?.Id ?? 0;
        var cart = await _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Discount)
            .FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null || cart.Items == null || cart.Items.Count == 0)
        {
            return RedirectToPage("/store/Cart");
        }

        // Resolve shipping address source
        string finalFullAddress = string.Empty;
        string finalPostalCode = string.Empty;

        if (SelectedAddressId.HasValue && user != null)
        {
            var selected = await _db.UserAddresses.FirstOrDefaultAsync(a => a.Id == SelectedAddressId.Value && a.UserId == user.Id);
            if (selected == null)
            {
                ModelState.AddModelError(string.Empty, "آدرس انتخاب‌شده معتبر نیست.");
                return Page();
            }
            finalFullAddress = selected.FullAddress;
            finalPostalCode = selected.PostalCode;
        }
        else
        {
            // Using new address from form
            if (string.IsNullOrWhiteSpace(Input.FullAddress) || string.IsNullOrWhiteSpace(Input.PostalCode))
            {
                ModelState.AddModelError(string.Empty, "آدرس کامل و کد پستی را وارد کنید.");
                return Page();
            }
            finalFullAddress = Input.FullAddress!;
            finalPostalCode = Input.PostalCode!;
        }

        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            TrackingCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
            TotalPrice = cart.Items.Sum(i =>
            {
                var price = i.Product.BasePrice;
                if (i.Product.Discount != null && i.Product.Discount.IsActive)
                {
                    price -= (price * i.Product.Discount.Percentage) / 100m;
                }
                return i.Quantity * price;
            }),
            ShippingAddress = new OrderAddress
            {
                FullName = Input.FullName,
                Phone = Input.Phone,
                City = Input.City,
                Street = Input.Street,
                FullAddress = finalFullAddress,
                PostalCode = finalPostalCode
            }
        };
        _db.Orders.Add(order);
        await _db.SaveChangesAsync();

        foreach (var it in cart.Items)
        {
            _db.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = it.ProductId,
                ProductSizeId = it.ProductSizeId,
                ProductColorId = it.ProductColorId,
                ProductMaterialId = it.ProductMaterialId,
                Quantity = it.Quantity,
                UnitPrice = (it.Product.Discount != null && it.Product.Discount.IsActive)
                    ? it.Product.BasePrice - ((it.Product.BasePrice * it.Product.Discount.Percentage) / 100m)
                    : it.Product.BasePrice
            });
        }
        await _db.SaveChangesAsync();

        // clear cart
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync();

        return RedirectToPage("/store/Payment", new { id = order.Id });
    }
}