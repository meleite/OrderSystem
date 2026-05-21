namespace OrderSystem.Models;

// Inheritance: inherits Id and CreatedAt from BaseEntity
public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Category { get; set; }

    // Navigation properties
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    // Polymorphism: meaningful override of BaseEntity.ToString()
    public override string ToString() => $"Product [Id={Id}, Name={Name}, Price={Price:C}]";
}
