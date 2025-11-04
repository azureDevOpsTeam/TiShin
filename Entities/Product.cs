namespace TiShinShop.Entities
{
    public class Product
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string Code { get; set; }

        public string Brand { get; set; }

        public string Description { get; set; }

        public string FirstImage { get; set; }

        public string SecondImage { get; set; }

        public decimal BasePrice { get; set; }

        public int Quantity { get; set; }

        public int? DiscountId { get; set; }

        public int Visit { get; set; }

        public DateTime CreateAt { get; set; } = DateTime.Now;

        public Discount Discount { get; set; }

        public ICollection<ProductCategory> ProductCategories { get; set; }

        public ICollection<ProductSize> Sizes { get; set; }

        public ICollection<ProductColor> Colors { get; set; }

        public ICollection<ProductMaterial> Materials { get; set; }

        public ICollection<ProductImage> Images { get; set; }
    }
}