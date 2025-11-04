using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.DTOs.Product;
using TiShinShop.Entities;

namespace TiShinShop.Pages.Store;

public class ProductModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public ProductModel(ApplicationDbContext db) { _db = db; }

    public Product? Item { get; set; }
    public List<ProductSize> Sizes { get; set; } = new();
    public List<ColorMaterialPair> ColorMaterialPairs { get; set; } = new();
    public List<ProductReview> Reviews { get; set; } = new();
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public decimal FinalPrice { get; set; }

    [BindProperty]
    public int SelectedSizeId { get; set; }

    [BindProperty]
    public List<AddToCartDto> CartItems { get; set; } = new();

    public class ColorMaterialPair
    {
        public Color Color { get; set; }
        public Material Material { get; set; }
        public int ProductColorId { get; set; }
        public int ProductMaterialId { get; set; }
    }

    public async Task<IActionResult> OnGet(int id)
    {
        Item = await _db.Products
            .Include(p => p.Sizes).ThenInclude(ps => ps.Size)
            .Include(p => p.Colors).ThenInclude(pc => pc.Color)
            .Include(p => p.Materials).ThenInclude(pm => pm.Material)
            .Include(p => p.Images)
            .Include(p => p.Discount)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (Item == null) return NotFound();
        Sizes = Item.Sizes.ToList();
        SelectedSizeId = Sizes.FirstOrDefault()?.Id ?? 0;

        // Build pairs (cross of product colors and materials)
        foreach (var pc in Item.Colors)
        {
            foreach (var pm in Item.Materials)
            {
                ColorMaterialPairs.Add(new ColorMaterialPair
                {
                    Color = pc.Color,
                    Material = pm.Material,
                    ProductColorId = pc.Id,
                    ProductMaterialId = pm.Id
                });
                CartItems.Add(new AddToCartDto
                {
                    ProductId = Item.Id,
                    ProductColorId = pc.Id,
                    ProductMaterialId = pm.Id,
                    ProductSizeId = SelectedSizeId,
                    Count = 0
                });
            }
        }

        // Reviews and rating
        Reviews = await _db.ProductReviews
            .Where(r => r.ProductId == id && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
        ReviewCount = Reviews.Count;
        AverageRating = ReviewCount > 0 ? Reviews.Average(r => r.Rating) : 0;

        // Price calculation with discount if available
        FinalPrice = Item.BasePrice;
        if (Item.Discount != null && Item.Discount.IsActive)
        {
            if (Item.Discount.FixedAmount.HasValue)
            {
                var reduction = Item.Discount.FixedAmount.Value;
                if (Item.Discount.MaxAmount.HasValue)
                    reduction = Math.Min(reduction, Item.Discount.MaxAmount.Value);
                FinalPrice = Math.Max(0, Item.BasePrice - reduction);
            }
            else
            {
                var pct = Item.Discount.Percentage;
                var reduction = Item.BasePrice * (pct / 100m);
                if (Item.Discount.MaxAmount.HasValue)
                    reduction = Math.Min(reduction, Item.Discount.MaxAmount.Value);
                FinalPrice = Math.Max(0, Item.BasePrice - reduction);
            }
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAddToCart(int id)
    {
        // Ensure product exists
        var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null) return NotFound();

        // Get or create cart for current user (guest=0)
        var userId = 0;
        var cart = await _db.Carts.Include(c => c.Items).FirstOrDefaultAsync(c => c.UserId == userId);
        if (cart == null)
        {
            cart = new Cart { UserId = userId, CreatedAt = DateTime.UtcNow };
            _db.Carts.Add(cart);
            await _db.SaveChangesAsync();
        }

        // Add items for combinations with Count > 0
        if (CartItems != null)
        {
            foreach (var item in CartItems.Where(ci => ci.Count > 0))
            {
                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = product.Id,
                    ProductColorId = item.ProductColorId,
                    ProductMaterialId = item.ProductMaterialId,
                    ProductSizeId = SelectedSizeId,
                    Quantity = item.Count
                };
                _db.CartItems.Add(cartItem);
            }
            await _db.SaveChangesAsync();
        }

        return RedirectToPage("/store/Cart");
    }
}