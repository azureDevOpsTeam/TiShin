using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiShinShop.Data;
using TiShinShop.Entities;
using TiShinShop.Extenssions.Enums;

namespace TiShinShop.Pages.Store;

public class PaymentModel : PageModel
{
    private readonly ApplicationDbContext _db;
    public PaymentModel(ApplicationDbContext db) { _db = db; }

    public Order? Order { get; set; }
    public decimal TotalDiscount { get; set; }
    public decimal FinalPrice { get; set; }
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public string? AppliedCode { get; set; }

    public async Task<IActionResult> OnGet(int id)
    {
        Order = await _db.Orders.Include(o => o.Coupon).FirstOrDefaultAsync(o => o.Id == id);
        if (Order == null) return NotFound();
        AppliedCode = Order.Coupon?.Code;
        ComputeTotals();
        return Page();
    }

    public async Task<IActionResult> OnPost(int id)
    {
        var order = await _db.Orders.FindAsync(id);
        if (order == null) return NotFound();
        order.Status = OrderStatus.Paid;
        await _db.SaveChangesAsync();
        return RedirectToPage("/store/SuccessPayment", new { id });
    }

    public async Task<IActionResult> OnPostApplyCoupon(int id, string code)
    {
        Order = await _db.Orders.Include(o => o.Coupon).FirstOrDefaultAsync(o => o.Id == id);
        if (Order == null) return NotFound();

        if (string.IsNullOrWhiteSpace(code))
        {
            IsError = true;
            Message = "کد تخفیف را وارد کنید";
            ComputeTotals();
            return Page();
        }

        var coupon = await _db.Coupons.FirstOrDefaultAsync(c => c.Code == code);
        if (coupon == null)
        {
            IsError = true;
            Message = "کد تخفیف معتبر نیست";
            ComputeTotals();
            return Page();
        }

        // Validate active and date window
        if (!coupon.IsActive || DateTime.UtcNow < coupon.StartDate || DateTime.UtcNow > coupon.EndDate)
        {
            IsError = true;
            Message = "این کد تخفیف در حال حاضر فعال نیست";
            ComputeTotals();
            return Page();
        }

        // Single-use coupon check: prevent reuse on paid orders
        if (coupon.IsSingleUse)
        {
            var usedOnPaidOrder = await _db.Orders.AnyAsync(o => o.CouponId == coupon.Id && o.Status == OrderStatus.Paid);
            if (usedOnPaidOrder)
            {
                IsError = true;
                Message = "این کد تخفیف قبلاً استفاده شده است";
                ComputeTotals();
                return Page();
            }
        }

        // Apply to order and persist
        Order.CouponId = coupon.Id;
        await _db.SaveChangesAsync();

        AppliedCode = coupon.Code;
        IsError = false;
        Message = "کد تخفیف با موفقیت اعمال شد";
        // Refresh navigation property for accurate calculation
        Order.Coupon = coupon;
        ComputeTotals();
        return Page();
    }

    private void ComputeTotals()
    {
        if (Order == null)
        {
            TotalDiscount = 0;
            FinalPrice = 0;
            return;
        }
        var couponDiscount = Order.Coupon?.CalculateDiscount(Order.TotalPrice) ?? 0m;
        var directDiscount = Order.DiscountAmount ?? 0m;
        TotalDiscount = directDiscount + couponDiscount;
        FinalPrice = Order.FinalPrice;
    }
}