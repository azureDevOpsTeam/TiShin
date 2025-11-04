using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;

namespace TiShinShop.ViewComponents;

public class CartItemCountViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    public CartItemCountViewComponent(ApplicationDbContext context)
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
                var itemCount = await _context.CartItems
                    .Where(ci => ci.Cart.UserId == user.Id)
                    .CountAsync();

                return View(itemCount);
            }
        }
        return View(0);
    }
}