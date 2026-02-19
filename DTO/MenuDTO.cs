using FoodDash.Enums;

namespace FoodDash.DTO
{
    public class MenuDTO
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public FoodCategory Category { get; set; }
        public bool IsAvailable { get; set; }
    }
}
