namespace TiShinShop.Entities
{
    public class Category
    {
        public int Id { get; set; }

        public int ParentId { get; set; }

        public string Name { get; set; }

        public bool IsBaseMenu { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}