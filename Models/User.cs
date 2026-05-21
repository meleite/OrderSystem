namespace OrderSystem.Models;

// Inheritance: inherits Id and CreatedAt from BaseEntity
public class User : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Encapsulation: PasswordHash cannot be set directly from outside
    public string PasswordHash { get; private set; } = string.Empty;

    public string Role { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }

    public void SetPasswordHash(string hash) => PasswordHash = hash;

    public ICollection<Order> Orders { get; set; } = new List<Order>();

    // Polymorphism: meaningful override of BaseEntity.ToString()
    public override string ToString() => $"User [Id={Id}, Name={Name}, Email={Email}]";
}
