namespace TiShinShop.DTOs.Product
{
    public class ProductHomeViewModel
    {
        public List<ListViewModel> NewProducts { get; set; } = new();

        public List<ListViewModel> DiscountProducts { get; set; } = new();

        public List<ListViewModel> TopVisitProducts { get; set; } = new();

        // Dynamic sections for homepage
        public List<ListViewModel> LatestLaptops { get; set; } = new();

        public List<ListViewModel> LatestClothes { get; set; } = new();

        // Best selling (approximate) products for homepage
        public List<ListViewModel> TopSellingProducts { get; set; } = new();
    }

    public class ListViewModel
    {
        public int ProductId { get; set; }

        public string Title { get; set; }

        public int Quantity { get; set; }

        public string FirstImage { get; set; }

        public string SecondImage { get; set; }

        public decimal BasePrice { get; set; }

        public ColorDataViewModel[] Colors { get; set; }
    }
    public class ColorDataViewModel
    {
        public string Title { get; set; }
        public string HexCode { get; set; }
    }
}