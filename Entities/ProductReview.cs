using System.ComponentModel.DataAnnotations;

namespace TiShinShop.Entities
{
    public class ProductReview
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public Product Product { get; set; }

        public int UserId { get; set; }

        public ApplicationUser User { get; set; }

        [Range(1,5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }

        public bool IsApproved { get; set; } = true;
    }
}