using TiShinShop.Extenssions.Enums;

namespace TiShinShop.Entities
{
    public class OrderStatusHistory
    {
        public int Id { get; set; }

        public int OrderId { get; set; }

        public Order Order { get; set; }

        public OrderStatus Status { get; set; }

        public DateTime ChangedAt { get; set; }

        public string Note { get; set; }
    }
}