namespace TiShinShop.DTOs.Product
{
    public class ProductListViewModel
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

        public int? Discount { get; set; }

        public string[] Colors { get; set; }
    }
}