using System;

namespace TiShinShop.Entities
{
    public class Vitrine
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
    }
}
