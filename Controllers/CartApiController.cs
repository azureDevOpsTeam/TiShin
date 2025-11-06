using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;

namespace TiShinShop.Controllers
{
    [ApiController]
    [Route("api")] // Ensures routes like /api/RemoveCartItem
    public class CartApiController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public CartApiController(ApplicationDbContext db)
        {
            _db = db;
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            var username = User?.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(username))
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == username);
                if (user != null) return user.Id;
            }
            return 0; // fallback for guest carts if any
        }

        private async Task<(decimal total, int count)> GetCartTotalsAsync(int userId)
        {
            var cart = await _db.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                        .ThenInclude(p => p.Discount)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart?.Items == null || cart.Items.Count == 0)
                return (0m, 0);

            decimal total = 0m;
            foreach (var item in cart.Items)
            {
                var price = item.Product?.BasePrice ?? 0m;
                if (item.Product?.Discount != null && item.Product.Discount.IsActive)
                {
                    var discount = item.Product.Discount.Percentage;
                    price -= (price * discount) / 100m;
                }
                total += price * item.Quantity;
            }
            var count = cart.Items.Count;
            return (total, count);
        }

        [HttpGet("RemoveCartItem")]
        public async Task<IActionResult> RemoveCartItem([FromQuery] int id)
        {
            var userId = await GetCurrentUserIdAsync();

            var item = await _db.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.UserId == userId);

            if (item == null)
                return NotFound(new { success = false, message = "آیتم یافت نشد" });

            _db.CartItems.Remove(item);
            await _db.SaveChangesAsync();

            var (total, count) = await GetCartTotalsAsync(userId);
            return Ok(new { success = true, total, count });
        }

        [HttpPost("IncrementCartItem")]
        public async Task<IActionResult> IncrementCartItem([FromForm] int id)
        {
            var userId = await GetCurrentUserIdAsync();
            var item = await _db.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product).ThenInclude(p => p.Discount)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.UserId == userId);
            if (item == null)
                return NotFound(new { success = false, message = "آیتم یافت نشد" });

            item.Quantity += 1;
            await _db.SaveChangesAsync();

            var price = item.Product?.BasePrice ?? 0m;
            if (item.Product?.Discount != null && item.Product.Discount.IsActive)
            {
                var discount = item.Product.Discount.Percentage;
                price -= (price * discount) / 100m;
            }
            var itemTotal = price * item.Quantity;

            var (total, count) = await GetCartTotalsAsync(userId);
            return Ok(new { success = true, quantity = item.Quantity, itemTotal, total, count });
        }

        [HttpPost("DecrementCartItem")]
        public async Task<IActionResult> DecrementCartItem([FromForm] int id)
        {
            var userId = await GetCurrentUserIdAsync();
            var item = await _db.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product).ThenInclude(p => p.Discount)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.UserId == userId);
            if (item == null)
                return NotFound(new { success = false, message = "آیتم یافت نشد" });

            if (item.Quantity > 1)
            {
                item.Quantity -= 1;
                await _db.SaveChangesAsync();
            }

            var price = item.Product?.BasePrice ?? 0m;
            if (item.Product?.Discount != null && item.Product.Discount.IsActive)
            {
                var discount = item.Product.Discount.Percentage;
                price -= (price * discount) / 100m;
            }
            var itemTotal = price * item.Quantity;

            var (total, count) = await GetCartTotalsAsync(userId);
            return Ok(new { success = true, quantity = item.Quantity, itemTotal, total, count });
        }

        [HttpPost("UpdateCartItemQuantity")]
        public async Task<IActionResult> UpdateCartItemQuantity([FromForm] int id, [FromForm] int quantity)
        {
            if (quantity < 1) quantity = 1;
            if (quantity > 100) quantity = 100;

            var userId = await GetCurrentUserIdAsync();
            var item = await _db.CartItems
                .Include(ci => ci.Cart)
                .Include(ci => ci.Product).ThenInclude(p => p.Discount)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.UserId == userId);
            if (item == null)
                return NotFound(new { success = false, message = "آیتم یافت نشد" });

            item.Quantity = quantity;
            await _db.SaveChangesAsync();

            var price = item.Product?.BasePrice ?? 0m;
            if (item.Product?.Discount != null && item.Product.Discount.IsActive)
            {
                var discount = item.Product.Discount.Percentage;
                price -= (price * discount) / 100m;
            }
            var itemTotal = price * item.Quantity;

            var (total, count) = await GetCartTotalsAsync(userId);
            return Ok(new { success = true, quantity = item.Quantity, itemTotal, total, count });
        }
    }
}