namespace TiShinShop.DTOs.Product
{
    public class ProductVariantDto
    {
        public int ProductId { get; set; }  // ارتباط با جدول محصولات

        public int[] SizeId { get; set; }  // ارتباط با سایز

        public int[] ColorId { get; set; }  // ارتباط با رنگ

        public int[] MaterialId { get; set; }  // ارتباط با جنس
    }
}