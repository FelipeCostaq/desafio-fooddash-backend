using FoodDash.Models;

namespace FoodDash.DTO
{
    public class OrdersDTO
    {
        public string CustomerName { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public List<ItemRequestDTO> Items { get; set; } = new List<ItemRequestDTO>();
    }

    public class ItemRequestDTO {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
