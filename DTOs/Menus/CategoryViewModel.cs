namespace TiShinShop.DTOs.Menus
{
    public class CategoryViewModel
    {
        public int Id { get; set; }

        public int ParentId { get; set; }

        public string Title { get; set; }

        public bool IsMenu { get; set; }

        public bool IsBase { get; set; }
    }
}