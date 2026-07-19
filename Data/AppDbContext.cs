using Microsoft.EntityFrameworkCore;
using PharMarket.Models.Entities;

namespace PharMarket.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Store> Stores => Set<Store>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Stock> Stocks => Set<Stock>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<Purchase> Purchases => Set<Purchase>();
    public DbSet<PurchaseItem> PurchaseItems => Set<PurchaseItem>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Capital> Capitals => Set<Capital>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<TaxSetting> TaxSettings => Set<TaxSetting>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Barcode)
            .IsUnique();

        modelBuilder.Entity<Product>()
            .HasQueryFilter(p => !p.IsDeleted);

        modelBuilder.Entity<Stock>()
            .HasIndex(s => s.ProductId)
            .IsUnique();

        modelBuilder.Entity<Sale>()
            .HasIndex(s => s.InvoiceNumber)
            .IsUnique();

        modelBuilder.Entity<Purchase>()
            .HasIndex(p => p.OrderNumber)
            .IsUnique();

        modelBuilder.Entity<Category>()
            .HasQueryFilter(c => !c.IsDeleted);

        modelBuilder.Entity<Supplier>()
            .HasQueryFilter(s => !s.IsDeleted);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Store>()
            .HasIndex(s => s.Name)
            .IsUnique();

        SeedDefaultData(modelBuilder);
    }

    private static void SeedDefaultData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Store>().HasData(
            new Store { Id = 1, Name = "Default Store", IsActive = true, CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "General", Description = "General products", StoreId = 1, CreatedAt = DateTime.UtcNow },
            new Category { Id = 2, Name = "Electronics", Description = "Electronic devices and accessories", StoreId = 1, CreatedAt = DateTime.UtcNow },
            new Category { Id = 3, Name = "Groceries", Description = "Food and grocery items", StoreId = 1, CreatedAt = DateTime.UtcNow },
            new Category { Id = 4, Name = "Household", Description = "Household essentials", StoreId = 1, CreatedAt = DateTime.UtcNow },
            new Category { Id = 5, Name = "Personal Care", Description = "Personal hygiene products", StoreId = 1, CreatedAt = DateTime.UtcNow }
        );

        modelBuilder.Entity<TaxSetting>().HasData(
            new TaxSetting { Id = 1, TaxName = "VAT", TaxRate = 7.5m, IsEnabled = true, StoreId = 1, CreatedAt = DateTime.UtcNow }
        );
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
