using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;

namespace TiShinShop.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;

        public CartViewComponent(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                var username = User.Identity.Name;
                var user = await _context.Users.FirstOrDefaultAsync(current => current.UserName == username);

                if (user != null)
                {
                    var cart = await _context.Carts
                        .Include(c => c.Items)
                            .ThenInclude(ci => ci.Product)
                        .Include(c => c.Items)
                            .ThenInclude(ci => ci.ProductSize)
                                .ThenInclude(ps => ps.Size)
                        .Include(c => c.Items)
                            .ThenInclude(ci => ci.ProductColor)
                                .ThenInclude(pc => pc.Color)
                        .FirstOrDefaultAsync(c => c.UserId == user.Id);

                    decimal total = 0;

                    if (cart?.Items != null)
                    {
                        foreach (var item in cart.Items)
                        {
                            var price = item.Product?.BasePrice ?? 0;

                            if (item.Product?.Discount != null && item.Product.Discount.IsActive)
                            {
                                var discount = item.Product.Discount.Percentage;
                                price -= (price * discount) / 100;
                            }

                            total += price * item.Quantity;
                        }
                    }

                    ViewData["CartTotal"] = total;

                    return View(cart);
                }
            }
            return View(new Cart());
        }
    }
}