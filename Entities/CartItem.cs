namespace TiShinShop.Entities
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }

        public int ProductId { get; set; }

        public int ProductSizeId { get; set; }

        public int ProductColorId { get; set; }

        public int ProductMaterialId { get; set; }

        public int Quantity { get; set; }

        public Cart Cart { get; set; }

        public Product Product { get; set; }

        public ProductSize ProductSize { get; set; }

        public ProductColor ProductColor { get; set; }

        public ProductMaterial ProductMaterial { get; set; }
    }
}