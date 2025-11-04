using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using TiShinShop.Extenssions.Enums;

namespace TiShinShop.Entities
{
    public class Order
    {
        public int Id { get; set; }

        public string TrackingCode { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public decimal TotalPrice { get; set; }

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public decimal? DiscountPercent { get; set; }

        public decimal? DiscountAmount { get; set; }

        public int? CouponId { get; set; }

        [ValidateNever]
        public Coupon Coupon { get; set; }

        public OrderAddress ShippingAddress { get; set; }

        [NotMapped]
        public bool IsCouponValid =>
        Coupon != null &&
        Coupon.IsActive &&
        DateTime.UtcNow >= Coupon.StartDate &&
        DateTime.UtcNow <= Coupon.EndDate;

        public ApplicationUser User { get; set; }

        public ICollection<OrderItem> Items { get; set; }

        public decimal FinalPrice => TotalPrice - (DiscountAmount ?? 0) - (Coupon?.CalculateDiscount(TotalPrice) ?? 0);
    }
}