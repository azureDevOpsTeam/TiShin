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

    public class InputModel
    {
        [Required] public string FullName { get; set; } = string.Empty;
        [Required] public string Phone { get; set; } = string.Empty;
        [Required] public string City { get; set; } = string.Empty;
        [Required] public string Street { get; set; } = string.Empty;
        [Required] public string FullAddress { get; set; } = string.Empty;
        [Required] public string PostalCode { get; set; } = string.Empty;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid) return Page();
        var userId = 0;
        var cart = await _db.Carts.Include(c => c.Items).ThenInclude(i => i.Product).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null || cart.Items == null || cart.Items.Count == 0)
        {
            return RedirectToPage("/store/Cart");
        }

        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            TrackingCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper(),
            TotalPrice = cart.Items.Sum(i => i.Quantity * i.Product.BasePrice),
            ShippingAddress = new OrderAddress
            {
                FullName = Input.FullName,
                Phone = Input.Phone,
                City = Input.City,
                Street = Input.Street,
                FullAddress = Input.FullAddress,
                PostalCode = Input.PostalCode
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
                UnitPrice = it.Product.BasePrice
            });
        }
        await _db.SaveChangesAsync();

        // clear cart
        _db.CartItems.RemoveRange(cart.Items);
        await _db.SaveChangesAsync();

        return RedirectToPage("/store/Payment", new { id = order.Id });
    }
}