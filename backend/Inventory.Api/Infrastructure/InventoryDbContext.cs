using Inventory.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Infrastructure;

public class InventoryDbContext(DbContextOptions<InventoryDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Code).IsRequired().HasMaxLength(50);
            entity.HasIndex(p => p.Code).IsUnique();
            entity.Property(p => p.Description).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Balance).IsRequired();
            entity.Property(p => p.XMin)
                .HasColumnType("xid")
                .HasColumnName("xmin")
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();
            entity.ToTable(t => t.HasCheckConstraint("CK_Products_Balance_NonNegative", "\"Balance\" >= 0"));
        });

        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.HasKey(r => r.Key);
            entity.Property(r => r.Key).HasMaxLength(200);
            entity.Property(r => r.ProductId).IsRequired();
            entity.Property(r => r.ProcessedDate).IsRequired();
            entity.HasIndex(r => r.ProcessedDate);
            entity.HasOne<Product>()
                .WithMany()
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
