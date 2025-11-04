namespace TiShinShop.Entities
{
    public class Color
    {
        public int Id { get; set; }

        public string Value { get; set; }

        public string HexCode { get; set; }

        public ICollection<ProductColor> ProductColors { get; set; }
    }
}