namespace TiShinShop.DTOs.Product
{
    public class AddToCartDto
    {
        public int ProductId { get; set; }
        public int ProductSizeId { get; set; }
        public int ProductColorId { get; set; }
        public int ProductMaterialId { get; set; }
        public int Count { get; set; }
    }
}
