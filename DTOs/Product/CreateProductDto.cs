namespace TiShinShop.DTOs.Product
{
    public class CreateProductDto
    {
        public string Title { get; set; }

        public string Code { get; set; }

        public string Brand { get; set; }

        public string Description { get; set; }

        public List<int> CategoriesId { get; set; }

        public decimal BasePrice { get; set; }

        public int Quantity { get; set; }

        public int DiscountPercentage { get; set; }

        public decimal? FixedAmount { get; set; }
    }
}