namespace TiShinShop.Entities
{
    public class Coupon
    {
        public int Id { get; set; }

        public string Code { get; set; }

        public decimal Percentage { get; set; } // درصد تخفیف (مثلاً 20)

        public decimal? MaxAmount { get; set; } // سقف تخفیف

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsSingleUse { get; set; } // فقط یک بار بشه استفاده کرد

        public bool IsActive { get; set; }

        public ICollection<Order> Orders { get; set; }

        public decimal CalculateDiscount(decimal total)
        {
            if (!IsActive || DateTime.UtcNow < StartDate || DateTime.UtcNow > EndDate)
                return 0;

            var discount = total * (Percentage / 100);
            return MaxAmount.HasValue ? Math.Min(MaxAmount.Value, discount) : discount;
        }
    }
}