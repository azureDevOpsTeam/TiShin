namespace TiShinShop.Entities
{
    public class Cart
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; }

        public ApplicationUser User { get; set; }

        public ICollection<CartItem> Items { get; set; }
    }
}