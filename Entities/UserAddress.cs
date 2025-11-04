namespace TiShinShop.Entities;

public class UserAddress
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string FullAddress { get; set; }

    public string PostalCode { get; set; }

    public bool IsDefault { get; set; }

    public ApplicationUser User { get; set; }
}