using FoodDash.Enums;

namespace FoodDash.DTO
{
    public class MenuEditDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public FoodCategory Category { get; set; }
    }
}
