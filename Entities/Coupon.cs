namespace TiShinShop.Entities
{
    public class Coupon
    {
        public int Id { get; set; }

        public string Code { get; set; }

        // یکی از این دو مقدار باید تنظیم شود: درصد یا مبلغ ثابت
        public decimal? Percentage { get; set; } // درصد تخفیف (مثلاً 20)

        public decimal? FixedAmount { get; set; } // مبلغ ثابت تخفیف

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
            decimal discount = 0m;

            if (FixedAmount.HasValue && FixedAmount.Value > 0)
            {
                discount = FixedAmount.Value;
            }
            else if (Percentage.HasValue && Percentage.Value > 0)
            {
                discount = total * (Percentage.Value / 100);
            }

            if (MaxAmount.HasValue)
            {
                discount = Math.Min(MaxAmount.Value, discount);
            }

            return Math.Max(0, discount);
        }
    }
}