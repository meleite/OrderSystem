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

    // Encapsulation: TotalAmount can only be updated via RecalculateTotal()
    public decimal TotalAmount { get; private set; }

    public void RecalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.Product.Price * i.Quantity);
    }

    // Navigation properties
    public User User { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

    // Polymorphism: meaningful override of BaseEntity.ToString()
    public override string ToString() => $"Order [Id={Id}, UserId={UserId}, Status={Status}, Total={TotalAmount:C}]";
}
