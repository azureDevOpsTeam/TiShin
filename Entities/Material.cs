namespace TiShinShop.Entities
{
    public class Material
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public ICollection<ProductMaterial> ProductMaterials { get; set; }
    }
}