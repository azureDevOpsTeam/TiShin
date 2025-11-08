namespace TiShinShop.DTOs.Cart
{
    public class CachedCartItem
    {
        public int ProductId { get; set; }
        public int ProductSizeId { get; set; }
        public int ProductColorId { get; set; }
        public int ProductMaterialId { get; set; }
        public int Quantity { get; set; }
    }
}