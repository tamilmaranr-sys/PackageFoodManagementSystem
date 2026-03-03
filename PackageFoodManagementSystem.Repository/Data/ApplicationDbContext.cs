using Microsoft.EntityFrameworkCore;
using PackageFoodManagementSystem.Repository.Models;

namespace PackageFoodManagementSystem.Repository.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<CustomerAddress> CustomerAddresses { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Bill> Bill { get; set; }
        public DbSet<Payment> Payment { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<UserAuthentication> UserAuthentications { get; set; }
        public DbSet<Batch> Batches { get; set; }
        public DbSet<Wallet> Wallets { get; set; }

        // Fixes CS1061 build errors in BatchController
        public DbSet<Category> Categories { get; set; }

        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Singular Table Mappings
            modelBuilder.Entity<Batch>().ToTable("Batch");
            modelBuilder.Entity<Bill>().ToTable("Bill");
            modelBuilder.Entity<Payment>().ToTable("Payment");
            modelBuilder.Entity<OrderItem>().ToTable("OrderItem");

            // Plural Table Mappings
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Category>().ToTable("Categories");
            modelBuilder.Entity<Cart>().ToTable("Carts");
            modelBuilder.Entity<CartItem>().ToTable("CartItems");

            // Decimal Precision
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");
            // 1. Payment -> Order (Break the direct path)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany()
                .HasForeignKey(p => p.OrderID)
                .OnDelete(DeleteBehavior.NoAction);

            // 2. Payment -> Bill (Break the indirect path through Bill)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Bill)
                .WithMany()
                .HasForeignKey(p => p.BillID) // Matches BillID in your Payment model
                .OnDelete(DeleteBehavior.NoAction);

            // 3. OrderItem -> Order (Keep this as well to prevent similar issues)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderID)
                .OnDelete(DeleteBehavior.NoAction);
            // 4. Inventory -> Product (Resolves the cycle/multiple cascade paths error)
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Product)
                .WithMany() // or .WithMany(p => p.Inventories) if you have that collection
                .HasForeignKey(i => i.ProductId)
                .OnDelete(DeleteBehavior.NoAction);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }
    }
}