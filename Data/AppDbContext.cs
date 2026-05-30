using Microsoft.EntityFrameworkCore;
using OrderSystem.Models;

namespace OrderSystem.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Name).IsRequired().HasMaxLength(100);
            e.Property(u => u.Email).IsRequired().HasMaxLength(200);
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).IsRequired().HasMaxLength(50);

            // Enforces uniqueness at the DB level and speeds up login lookups.
            e.HasIndex(u => u.Email).IsUnique().HasDatabaseName("UX_Users_Email");
        });

        // Product
        modelBuilder.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).IsRequired().HasMaxLength(200);
            e.Property(p => p.Price).HasColumnType("decimal(18,2)");
            e.Property(p => p.Category).HasMaxLength(100);  // required to be indexable (no max columns in SQL Server indexes)

            // Supports ORDER BY Name and prefix searches.
            e.HasIndex(p => p.Name)
             .HasDatabaseName("IX_Products_Name");

            // Covers the most common public filter: active products in a category.
            e.HasIndex(p => new { p.IsActive, p.Category })
             .HasDatabaseName("IX_Products_IsActive_Category");

            // Covers price-range filters on active products.
            e.HasIndex(p => new { p.IsActive, p.Price })
             .HasDatabaseName("IX_Products_IsActive_Price");
        });

        // Order
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.TotalPrice).HasColumnType("decimal(18,2)");
            e.Property(o => o.Status).HasConversion<string>().HasMaxLength(50);

            e.HasOne(o => o.User)
             .WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // Composite (UserId, CreatedAt): covers "my orders" queries sorted/filtered by date.
            // UserId is the leftmost column so it also satisfies FK lookups, suppressing the
            // auto-generated plain IX_Orders_UserId index.
            e.HasIndex(o => new { o.UserId, o.CreatedAt })
             .HasDatabaseName("IX_Orders_UserId_CreatedAt");

            // Standalone CreatedAt for admin/reporting queries that range across all orders.
            e.HasIndex(o => o.CreatedAt)
             .HasDatabaseName("IX_Orders_CreatedAt");
        });

        // OrderItem
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(oi => oi.Id);
            e.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
            e.Property(oi => oi.LineTotal).HasColumnType("decimal(18,2)");

            e.HasOne(oi => oi.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(oi => oi.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(oi => oi.Product)
             .WithMany(p => p.OrderItems)
             .HasForeignKey(oi => oi.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
