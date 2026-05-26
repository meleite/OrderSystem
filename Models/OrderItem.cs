namespace OrderSystem.Models;

// Inheritance: inherits Id and CreatedAt from BaseEntity
public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public decimal LineTotal { get; set; }

    // Navigation properties
    public Order Order { get; set; } = null!;
    public Product Product { get; set; } = null!;

    // Polymorphism: meaningful override of BaseEntity.ToString()
    public override string ToString() => $"OrderItem [Id={Id}, ProductId={ProductId}, Qty={Quantity}]";
}
