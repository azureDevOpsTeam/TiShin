namespace TiShinShop.DTOs.Order
{
    public class OrderDetailsDto
    {
        public int OrderId { get; set; }

        public string Fullname { get; set; }

        public string PhoneNumber { get; set; }

        public string Street { get; set; }

        public string FullAddress { get; set; }

        public string PostalCodde { get; set; }

        public List<OrderDetailsItemDto> OrderDetailsItem { get; set; }
    }

    public class OrderDetailsItemDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string Color { get; set; }

        public string Size { get; set; }

        public string Material { get; set; }

        public int Quantity { get; set; }
    }
}