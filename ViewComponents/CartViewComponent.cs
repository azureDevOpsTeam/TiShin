using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;
using TiShinShop.Services;

namespace TiShinShop.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly IGuestCartService _guestCartService;

        public CartViewComponent(ApplicationDbContext context, IGuestCartService guestCartService)
        {
            _context = context;
            _guestCartService = guestCartService;
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
            // Guest cart total
            var cartId = _guestCartService.GetOrCreateGuestCartId(HttpContext);
            var (guestTotal, _) = await _guestCartService.GetTotalsAsync(cartId);
            ViewData["CartTotal"] = guestTotal;
            return View(new Cart());
        }
    }
}