using Microsoft.AspNetCore.Http;
using TiShinShop.DTOs.Cart;

namespace TiShinShop.Services
{
    public interface IGuestCartService
    {
        string GetOrCreateGuestCartId(HttpContext httpContext);
        Task<List<CachedCartItem>> GetItemsAsync(string cartId);
        Task SaveItemsAsync(string cartId, List<CachedCartItem> items, TimeSpan? ttl = null);
        Task AddItemsAsync(string cartId, IEnumerable<CachedCartItem> newItems);
        Task<int> IncrementAsync(string cartId, int productId, int sizeId, int colorId, int materialId);
        Task<int> DecrementAsync(string cartId, int productId, int sizeId, int colorId, int materialId);
        Task<bool> RemoveAsync(string cartId, int productId, int sizeId, int colorId, int materialId);
        Task<(decimal total, int count)> GetTotalsAsync(string cartId);
        Task MigrateToUserCartAsync(HttpContext httpContext, int userId);
        Task ClearAsync(string cartId);
    }
}