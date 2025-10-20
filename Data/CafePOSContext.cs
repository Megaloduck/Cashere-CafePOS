// Data/CafePOSContext.cs - Entity Framework DbContext

using Microsoft.EntityFrameworkCore;
using CafePOS.Core.Models;

namespace CafePOS.Data
{
    public class CafePOSContext : DbContext
    {
        public CafePOSContext(DbContextOptions<CafePOSContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<MenuCategory> MenuCategories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<TaxSettings> TaxSettings { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<DailySummary> DailySummaries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // User Configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(255);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // MenuCategory Configuration
            modelBuilder.Entity<MenuCategory>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            // MenuItem Configuration
            modelBuilder.Entity<MenuItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasPrecision(10, 2);
                entity.Property(e => e.CustomTaxRate).HasPrecision(5, 4);

                entity.HasOne(e => e.Category)
                    .WithMany(c => c.MenuItems)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.CategoryId);
            });

            // TaxSettings Configuration
            modelBuilder.Entity<TaxSettings>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.DefaultTaxRate).HasPrecision(5, 4);
                entity.Property(e => e.TaxName).IsRequired().HasMaxLength(50);
            });

            // Order Configuration
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.SubtotalAmount).HasPrecision(12, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(12, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(12, 2);
                entity.Property(e => e.DiscountAmount).HasPrecision(12, 2);

                entity.HasOne(e => e.Cashier)
                    .WithMany()
                    .HasForeignKey(e => e.CashierId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Transaction)
                    .WithOne(t => t.Order)
                    .HasForeignKey<Transaction>(t => t.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => e.OrderNumber).IsUnique();
                entity.HasIndex(e => e.OrderDate);
                entity.HasIndex(e => e.CashierId);
            });

            // OrderItem Configuration
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ItemName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
                entity.Property(e => e.SubtotalAmount).HasPrecision(12, 2);
                entity.Property(e => e.TaxAmount).HasPrecision(12, 2);
                entity.Property(e => e.TotalAmount).HasPrecision(12, 2);
                entity.Property(e => e.TaxRate).HasPrecision(5, 4);

                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.MenuItem)
                    .WithMany()
                    .HasForeignKey(e => e.MenuItemId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(e => e.OrderId);
                entity.HasIndex(e => e.MenuItemId);
            });

            // Transaction Configuration
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AmountPaid).HasPrecision(12, 2);
                entity.Property(e => e.ChangeAmount).HasPrecision(12, 2);
                entity.Property(e => e.ReferenceNumber).HasMaxLength(100);
                entity.Property(e => e.QRCodeData).IsRequired(false);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasIndex(e => e.OrderId).IsUnique();
                entity.HasIndex(e => e.ReferenceNumber);
                entity.HasIndex(e => e.TransactionDate);
            });

            // DailySummary Configuration
            modelBuilder.Entity<DailySummary>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TotalRevenue).HasPrecision(14, 2);
                entity.Property(e => e.TotalTax).HasPrecision(14, 2);
                entity.Property(e => e.CashCollected).HasPrecision(14, 2);
                entity.Property(e => e.QRISCollected).HasPrecision(14, 2);

                entity.HasIndex(e => e.SummaryDate).IsUnique();
            });

            // Seed initial data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed default tax settings (10% VAT)
            modelBuilder.Entity<TaxSettings>().HasData(
                new TaxSettings
                {
                    Id = 1,
                    DefaultTaxRate = 0.10m,
                    TaxName = "PPN",
                    IsEnabled = true,
                    UpdatedAt = DateTime.UtcNow
                }
            );

            // Seed menu categories
            modelBuilder.Entity<MenuCategory>().HasData(
                new MenuCategory { Id = 1, Name = "Coffee", Description = "Hot & Cold Coffee", DisplayOrder = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                new MenuCategory { Id = 2, Name = "Tea", Description = "Tea Beverages", DisplayOrder = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                new MenuCategory { Id = 3, Name = "Pastry", Description = "Cakes & Pastries", DisplayOrder = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
                new MenuCategory { Id = 4, Name = "Sandwich", Description = "Bread & Sandwiches", DisplayOrder = 4, IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            // Seed menu items
            modelBuilder.Entity<MenuItem>().HasData(
                new MenuItem { Id = 1, CategoryId = 1, Name = "Americano", Description = "Espresso with hot water", Price = 25000m, IsTaxable = true, IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow },
                new MenuItem { Id = 2, CategoryId = 1, Name = "Cappuccino", Description = "Espresso with steamed milk", Price = 30000m, IsTaxable = true, IsActive = true, DisplayOrder = 2, CreatedAt = DateTime.UtcNow },
                new MenuItem { Id = 3, CategoryId = 1, Name = "Latte", Description = "Espresso with lots of steamed milk", Price = 32000m, IsTaxable = true, IsActive = true, DisplayOrder = 3, CreatedAt = DateTime.UtcNow },
                new MenuItem { Id = 4, CategoryId = 2, Name = "Green Tea", Description = "Fresh green tea", Price = 18000m, IsTaxable = true, IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow },
                new MenuItem { Id = 5, CategoryId = 3, Name = "Croissant", Description = "Buttery croissant", Price = 22000m, IsTaxable = true, IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow },
                new MenuItem { Id = 6, CategoryId = 4, Name = "Ham & Cheese Sandwich", Description = "Toasted sandwich with ham and cheese", Price = 35000m, IsTaxable = true, IsActive = true, DisplayOrder = 1, CreatedAt = DateTime.UtcNow }
            );

            // Seed a default admin user (password: admin123 - hashed, you should generate this properly)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    // BCrypt hash of "admin123":
                    PasswordHash = "$2a$11$N9qo8uLOickgx2ZMRZoMyeIjZAgcg7b3XeKeUxWdeS86AGR0Ky/zq",
                    Email = "admin@cafe.com",
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}