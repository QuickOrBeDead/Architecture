using Microsoft.EntityFrameworkCore;
using RestaurantManagement.Api.Entities;

namespace RestaurantManagement.Api.Data;

public class RestaurantDbContext(DbContextOptions<RestaurantDbContext> options) : DbContext(options)
{
    public DbSet<Table> Tables => Set<Table>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Table configuration
        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TableNumber).IsRequired();
            entity.Property(e => e.Capacity).IsRequired();
            entity.Property(e => e.Status).IsRequired();
        });

        // MenuItem configuration
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).IsRequired().HasPrecision(18, 2);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).HasMaxLength(25).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.HasOne<Table>()
                .WithMany()
                .HasForeignKey(e => e.TableId);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.SpecialInstructions).HasMaxLength(250);
            entity.HasOne<Order>()
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<MenuItem>()
                .WithMany()
                .HasForeignKey(e => e.MenuItemId);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Tables
        modelBuilder.Entity<Table>().HasData(
            new Table { Id = 1, TableNumber = 1, Capacity = 4, Status = TableStatus.Available },
            new Table { Id = 2, TableNumber = 2, Capacity = 2, Status = TableStatus.Available },
            new Table { Id = 3, TableNumber = 3, Capacity = 6, Status = TableStatus.Available },
            new Table { Id = 4, TableNumber = 4, Capacity = 4, Status = TableStatus.Available },
            new Table { Id = 5, TableNumber = 5, Capacity = 8, Status = TableStatus.Available }
        );

        // Seed MenuItems
        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem { Id = 1, Name = "Margherita Pizza", Category = "Pizza", Price = 12.99m, IsAvailable = true },
            new MenuItem { Id = 2, Name = "Pepperoni Pizza", Category = "Pizza", Price = 14.99m, IsAvailable = true },
            new MenuItem { Id = 3, Name = "Caesar Salad", Category = "Salad", Price = 8.99m, IsAvailable = true },
            new MenuItem { Id = 4, Name = "Pasta Carbonara", Category = "Pasta", Price = 13.99m, IsAvailable = true },
            new MenuItem { Id = 5, Name = "Grilled Chicken", Category = "Main Course", Price = 16.99m, IsAvailable = true },
            new MenuItem { Id = 6, Name = "Tiramisu", Category = "Dessert", Price = 6.99m, IsAvailable = true },
            new MenuItem { Id = 7, Name = "Coca Cola", Category = "Beverage", Price = 2.99m, IsAvailable = true },
            new MenuItem { Id = 8, Name = "Espresso", Category = "Beverage", Price = 3.49m, IsAvailable = true }
        );
    }
}
