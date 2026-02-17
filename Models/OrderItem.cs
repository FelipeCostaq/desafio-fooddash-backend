using System.ComponentModel.DataAnnotations.Schema;

namespace FoodDash.Models
{
    public class OrderItem
    {
        public int Id { get; set; } 
        public Guid OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Orders ?Order { get; set; } 
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Menu ?Product { get; set; } 
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}