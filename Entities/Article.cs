namespace TiShinShop.Entities
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Summary { get; set; }
        public string ContentHtml { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsPublished { get; set; } = true;
        public int? AuthorId { get; set; }
        public ApplicationUser Author { get; set; }
        public int Views { get; set; }
    }
}
