using Microsoft.EntityFrameworkCore;

namespace TiShinShop.Entities
{
    [Owned]
    public class OrderAddress
    {
        public int Id { get; set; }

        public string FullName { get; set; }

        public string Phone { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public string FullAddress { get; set; }

        public string PostalCode { get; set; }
    }
}