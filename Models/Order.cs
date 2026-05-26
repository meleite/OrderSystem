namespace OrderSystem.Models;

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled
}

// Inheritance: inherits Id and CreatedAt from BaseEntity
public class Order : BaseEntity
{
    public int UserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string? ShippingAddress { get; set; }

    // Encapsulation: TotalPrice can only be updated via RecalculateTotal()
    public decimal TotalPrice { get; private set; }

    public void RecalculateTotal()
    {
        TotalPrice = Items.Sum(i => i.UnitPrice * i.Quantity);
    }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    // Polymorphism: meaningful override of BaseEntity.ToString()
    public override string ToString() => $"Order [Id={Id}, UserId={UserId}, Status={Status}, Total={TotalPrice:C}]";
}
