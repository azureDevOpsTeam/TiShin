using TiShinShop.Extenssions.Enums;

namespace TiShinShop.Entities
{
    public class Size
    {
        public int Id { get; set; }

        public SizeType SizeType { get; set; }

        public string Value { get; set; }

        public ICollection<ProductSize> ProductSizes { get; set; }
    }
}