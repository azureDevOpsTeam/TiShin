namespace TiShinShop.Entities
{
    public class Discount
    {
        public int Id { get; set; }

        public int Percentage { get; set; }

        public decimal? FixedAmount { get; set; }

        public decimal? MaxAmount { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsActive => !StartDate.HasValue || DateTime.UtcNow >= StartDate
            && (!EndDate.HasValue || DateTime.UtcNow <= EndDate);

        public ICollection<Order> Orders { get; set; }
    }
}