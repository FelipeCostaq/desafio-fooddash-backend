using FoodDash.Enums;

public class Menu
{
    public int Id { get; set; } 
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public FoodCategory Category { get; set; }
    public bool IsAvailable { get; set; }
}