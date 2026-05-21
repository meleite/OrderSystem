namespace OrderSystem.Models;

// To allow abstraction on the project moving Id and DateTime to a common class for all entities
public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Polymorphism: each subclass can override for a meaningful string
    public override string ToString() => $"{GetType().Name} [Id={Id}]";
}
