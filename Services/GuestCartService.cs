using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using TiShinShop.Data;
using TiShinShop.DTOs.Cart;
using TiShinShop.Entities;

namespace TiShinShop.Services
{
    public class GuestCartService : IGuestCartService
    {
        private const string CookieName = "GuestCartId";
        private readonly IDistributedCache _cache;
        private readonly ApplicationDbContext _db;

        public GuestCartService(IDistributedCache cache, ApplicationDbContext db)
        {
            _cache = cache;
            _db = db;
        }

        public string GetOrCreateGuestCartId(HttpContext httpContext)
        {
            if (httpContext.Request.Cookies.TryGetValue(CookieName, out var cartId) && !string.IsNullOrWhiteSpace(cartId))
            {
                return cartId;
            }
            cartId = Guid.NewGuid().ToString("N");
            httpContext.Response.Cookies.Append(CookieName, cartId, new CookieOptions
            {
                HttpOnly = false,
                IsEssential = true,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(14)
            });
            return cartId;
        }

        private static string Key(string cartId) => $"guestcart:{cartId}";

        public async Task<List<CachedCartItem>> GetItemsAsync(string cartId)
        {
            var data = await _cache.GetStringAsync(Key(cartId));
            if (string.IsNullOrWhiteSpace(data)) return new List<CachedCartItem>();
            try
            {
                var items = JsonSerializer.Deserialize<List<CachedCartItem>>(data) ?? new List<CachedCartItem>();
                return items;
            }
            catch
            {
                return new List<CachedCartItem>();
            }
        }

        public async Task SaveItemsAsync(string cartId, List<CachedCartItem> items, TimeSpan? ttl = null)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = ttl ?? TimeSpan.FromDays(14)
            };
            var payload = JsonSerializer.Serialize(items);
            await _cache.SetStringAsync(Key(cartId), payload, options);
        }

        public async Task AddItemsAsync(string cartId, IEnumerable<CachedCartItem> newItems)
        {
            var items = await GetItemsAsync(cartId);
            foreach (var it in newItems)
            {
                var existing = items.FirstOrDefault(x => x.ProductId == it.ProductId && x.ProductSizeId == it.ProductSizeId && x.ProductColorId == it.ProductColorId && x.ProductMaterialId == it.ProductMaterialId);
                if (existing == null)
                {
                    items.Add(new CachedCartItem
                    {
                        ProductId = it.ProductId,
                        ProductSizeId = it.ProductSizeId,
                        ProductColorId = it.ProductColorId,
                        ProductMaterialId = it.ProductMaterialId,
                        Quantity = it.Quantity
                    });
                }
                else
                {
                    existing.Quantity += it.Quantity;
                }
            }
            await SaveItemsAsync(cartId, items);
        }

        public async Task<int> IncrementAsync(string cartId, int productId, int sizeId, int colorId, int materialId)
        {
            var items = await GetItemsAsync(cartId);
            var existing = items.FirstOrDefault(x => x.ProductId == productId && x.ProductSizeId == sizeId && x.ProductColorId == colorId && x.ProductMaterialId == materialId);
            if (existing == null)
            {
                existing = new CachedCartItem { ProductId = productId, ProductSizeId = sizeId, ProductColorId = colorId, ProductMaterialId = materialId, Quantity = 1 };
                items.Add(existing);
            }
            else
            {
                existing.Quantity += 1;
            }
            await SaveItemsAsync(cartId, items);
            return existing.Quantity;
        }

        public async Task<int> DecrementAsync(string cartId, int productId, int sizeId, int colorId, int materialId)
        {
            var items = await GetItemsAsync(cartId);
            var existing = items.FirstOrDefault(x => x.ProductId == productId && x.ProductSizeId == sizeId && x.ProductColorId == colorId && x.ProductMaterialId == materialId);
            if (existing == null)
            {
                return 0;
            }
            if (existing.Quantity > 1) existing.Quantity -= 1;
            await SaveItemsAsync(cartId, items);
            return existing.Quantity;
        }

        public async Task<bool> RemoveAsync(string cartId, int productId, int sizeId, int colorId, int materialId)
        {
            var items = await GetItemsAsync(cartId);
            var removed = items.RemoveAll(x => x.ProductId == productId && x.ProductSizeId == sizeId && x.ProductColorId == colorId && x.ProductMaterialId == materialId) > 0;
            await SaveItemsAsync(cartId, items);
            return removed;
        }

        public async Task<(decimal total, int count)> GetTotalsAsync(string cartId)
        {
            var items = await GetItemsAsync(cartId);
            if (items.Count == 0) return (0m, 0);

            decimal total = 0m;
            foreach (var it in items)
            {
                var product = await _db.Products.Include(p => p.Discount).FirstOrDefaultAsync(p => p.Id == it.ProductId);
                decimal price = product?.BasePrice ?? 0m;
                if (product?.Discount != null && product.Discount.IsActive)
                {
                    var discount = product.Discount.Percentage;
                    price -= (price * discount) / 100m;
                }
                total += price * it.Quantity;
            }
            return (total, items.Count);
        }

        public async Task MigrateToUserCartAsync(HttpContext httpContext, int userId)
        {
            if (!httpContext.Request.Cookies.TryGetValue(CookieName, out var cartId) || string.IsNullOrWhiteSpace(cartId))
                return;

            var items = await GetItemsAsync(cartId);
            if (items.Count == 0)
                return;

            var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null)
            {
                cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
                _db.Carts.Add(cart);
                await _db.SaveChangesAsync();
            }

            foreach (var it in items)
            {
                var existing = await _db.CartItems.FirstOrDefaultAsync(ci => ci.CartId == cart.Id && ci.ProductId == it.ProductId && ci.ProductSizeId == it.ProductSizeId && ci.ProductColorId == it.ProductColorId && ci.ProductMaterialId == it.ProductMaterialId);
                if (existing == null)
                {
                    _db.CartItems.Add(new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = it.ProductId,
                        ProductSizeId = it.ProductSizeId,
                        ProductColorId = it.ProductColorId,
                        ProductMaterialId = it.ProductMaterialId,
                        Quantity = it.Quantity
                    });
                }
                else
                {
                    existing.Quantity += it.Quantity;
                }
            }
            await _db.SaveChangesAsync();

            await ClearAsync(cartId);
            // Remove cookie after migration
            httpContext.Response.Cookies.Delete(CookieName);
        }

        public async Task ClearAsync(string cartId)
        {
            await _cache.RemoveAsync(Key(cartId));
        }
    }
}