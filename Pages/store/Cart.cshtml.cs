using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;
using TiShinShop.Services;
using TiShinShop.DTOs.Cart;

namespace TiShinShop.Pages.Store;

public class CartModel : PageModel
{
    private readonly ApplicationDbContext _db;
    private readonly IGuestCartService _guestCartService;
    public CartModel(ApplicationDbContext db, IGuestCartService guestCartService) { _db = db; _guestCartService = guestCartService; }

    public IList<CartItem> Items { get; set; } = new List<CartItem>();
    public IList<GuestCartItemView> GuestItems { get; set; } = new List<GuestCartItemView>();
    public bool IsAuthenticated { get; set; }

    public decimal TotalBasePrice { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal TotalFinalPrice { get; set; }

    public class GuestCartItemView
    {
        public int ProductId { get; set; }
        public int ProductSizeId { get; set; }
        public int ProductColorId { get; set; }
        public int ProductMaterialId { get; set; }
        public int Quantity { get; set; }

        public Product Product { get; set; } = default!;
        public ProductSize ProductSize { get; set; } = default!;
        public ProductColor ProductColor { get; set; } = default!;
        public ProductMaterial ProductMaterial { get; set; } = default!;
    }

    public async Task OnGet()
    {
        var username = User?.Identity?.Name;
        var user = !string.IsNullOrWhiteSpace(username)
            ? await _db.Users.FirstOrDefaultAsync(u => u.UserName == username)
            : null;
        IsAuthenticated = user != null;

        if (IsAuthenticated)
        {
            var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == user!.Id);
            if (cart != null)
            {
                Items = await _db.CartItems
                    .Include(ci => ci.Product).ThenInclude(p => p.Discount)
                    .Include(ci => ci.Product).ThenInclude(p => p.Images)
                    .Include(ci => ci.ProductSize).ThenInclude(ps => ps.Size)
                    .Include(ci => ci.ProductColor).ThenInclude(pc => pc.Color)
                    .Include(ci => ci.ProductMaterial).ThenInclude(pm => pm.Material)
                    .Where(ci => ci.CartId == cart.Id)
                    .ToListAsync();
            }
        }
        else
        {
            var cartId = _guestCartService.GetOrCreateGuestCartId(HttpContext);
            var cached = await _guestCartService.GetItemsAsync(cartId);
            foreach (var it in cached)
            {
                var vm = new GuestCartItemView
                {
                    ProductId = it.ProductId,
                    ProductSizeId = it.ProductSizeId,
                    ProductColorId = it.ProductColorId,
                    ProductMaterialId = it.ProductMaterialId,
                    Quantity = it.Quantity,
                    Product = await _db.Products.Include(p => p.Discount).Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == it.ProductId) ?? new Product(),
                    ProductSize = await _db.ProductSizes.Include(ps => ps.Size).FirstOrDefaultAsync(ps => ps.Id == it.ProductSizeId) ?? new ProductSize { Size = new Size() },
                    ProductColor = await _db.ProductColors.Include(pc => pc.Color).FirstOrDefaultAsync(pc => pc.Id == it.ProductColorId) ?? new ProductColor { Color = new Color() },
                    ProductMaterial = await _db.ProductMaterials.Include(pm => pm.Material).FirstOrDefaultAsync(pm => pm.Id == it.ProductMaterialId) ?? new ProductMaterial { Material = new Material() }
                };
                GuestItems.Add(vm);
            }
        }

        // Calculate totals
        if (IsAuthenticated)
        {
            TotalBasePrice = Items.Sum(it => it.Product.BasePrice * it.Quantity);
            TotalDiscount = Items.Sum(it => CalculateUnitDiscount(it.Product) * it.Quantity);
            TotalFinalPrice = TotalBasePrice - TotalDiscount;
        }
        else
        {
            TotalBasePrice = GuestItems.Sum(it => it.Product.BasePrice * it.Quantity);
            TotalDiscount = GuestItems.Sum(it => CalculateUnitDiscount(it.Product) * it.Quantity);
            TotalFinalPrice = TotalBasePrice - TotalDiscount;
        }
    }

    public static decimal CalculateUnitDiscount(Product product)
    {
        if (product?.Discount != null && product.Discount.IsActive)
        {
            if (product.Discount.FixedAmount.HasValue)
            {
                var reduction = product.Discount.FixedAmount.Value;
                if (product.Discount.MaxAmount.HasValue)
                    reduction = Math.Min(reduction, product.Discount.MaxAmount.Value);
                return Math.Max(0, reduction);
            }
            else
            {
                var pct = product.Discount.Percentage;
                var reduction = product.BasePrice * (pct / 100m);
                if (product.Discount.MaxAmount.HasValue)
                    reduction = Math.Min(reduction, product.Discount.MaxAmount.Value);
                return Math.Max(0, reduction);
            }
        }
        return 0m;
    }

    public async Task<IActionResult> OnPostRemove(int id, int productId, int productSizeId, int productColorId, int productMaterialId)
    {
        var username = User?.Identity?.Name;
        var user = !string.IsNullOrWhiteSpace(username)
            ? await _db.Users.FirstOrDefaultAsync(u => u.UserName == username)
            : null;
        if (user != null)
        {
            var item = await _db.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == id && ci.Cart.UserId == user.Id);
            if (item != null)
            {
                _db.CartItems.Remove(item);
                await _db.SaveChangesAsync();
            }
        }
        else
        {
            var cartId = _guestCartService.GetOrCreateGuestCartId(HttpContext);
            await _guestCartService.RemoveAsync(cartId, productId, productSizeId, productColorId, productMaterialId);
        }
        return RedirectToPage();
    }
}