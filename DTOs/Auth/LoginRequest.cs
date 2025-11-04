namespace TiShinShop.DTOs.Auth
{
    using System.ComponentModel.DataAnnotations;

    public class LoginRequest
    {
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
