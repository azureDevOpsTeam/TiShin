using Microsoft.AspNetCore.Identity;

namespace TiShinShop.Entities
{
    public class ApplicationUser : IdentityUser<int>
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string VerificationCode { get; set; }

        public DateTime ExpireCode { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsPhoneNumberConfirmed { get; set; } = false;

        public ICollection<Order> Orders { get; set; }

        public ICollection<Cart> Carts { get; set; }
    }
}