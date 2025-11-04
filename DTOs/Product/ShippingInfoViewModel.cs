namespace TiShinShop.DTOs.Product;

public class ShippingInfoViewModel
{
    public string FullName { get; set; }
    public string PhoneNumber { get; set; }
    public string Address { get; set; }
    public string PostalCode { get; set; }

    public List<DeliveryMethodDto> DeliveryMethods { get; set; }
    public List<CartItemDto> CartItems { get; set; }

    public List<DeliveryDayDto> AvailableDays { get; set; }
    public List<DeliveryTimeSlotDto> TimeSlots { get; set; }

    public decimal ShippingCost { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalAmount => TotalPrice - DiscountAmount + ShippingCost;
}

public class CartItemDto
{
    public string Title { get; set; }
    public string Image { get; set; }
    public string Color { get; set; }
    public string Size { get; set; }
    public int Quantity { get; set; }
}

public class DeliveryMethodDto
{
    public string Name { get; set; }
    public string Type { get; set; } // "normal", "pickup"
    public decimal Cost { get; set; }
}

public class DeliveryDayDto
{
    public string Day { get; set; }
    public string Date { get; set; }
}

public class DeliveryTimeSlotDto
{
    public string From { get; set; }
    public string To { get; set; }
}
