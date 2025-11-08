using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Services;

namespace TiShinShop.ViewComponents;

public class CartItemCountViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;
    private readonly IGuestCartService _guestCartService;

    public CartItemCountViewComponent(ApplicationDbContext context, IGuestCartService guestCartService)
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
                var itemCount = await _context.CartItems
                    .Where(ci => ci.Cart.UserId == user.Id)
                    .CountAsync();

                return View(itemCount);
            }
        }
        // guest cart count
        var cartId = _guestCartService.GetOrCreateGuestCartId(HttpContext);
        var items = await _guestCartService.GetItemsAsync(cartId);
        return View(items.Count);
    }
}