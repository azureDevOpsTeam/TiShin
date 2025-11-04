namespace TiShinShop.Entities
{
    public class ProductSize
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int SizeId { get; set; }

        public Size Size { get; set; }

        public Product Product { get; set; }
    }

    public class ProductColor
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int ColorId { get; set; }

        public Color Color { get; set; }

        public Product Product { get; set; }
    }

    public class ProductMaterial
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public int MaterialId { get; set; }

        public Material Material { get; set; }

        public Product Product { get; set; }
    }
}