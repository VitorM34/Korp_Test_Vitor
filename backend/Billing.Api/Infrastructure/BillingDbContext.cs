using Billing.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Billing.Api.Infrastructure;

public class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoicePrintLog> InvoicePrintLogs => Set<InvoicePrintLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // InvoiceItem é entidade filha do agregado Invoice: sem DbSet próprio,
        // todo acesso/escrita deve passar pela raiz do agregado (Invoice).
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Number).IsRequired();
            entity.HasIndex(n => n.Number).IsUnique();
            entity.Property(n => n.Status).IsRequired().HasConversion<string>();
            entity.Property(n => n.CreatedDate).IsRequired();
            entity.Property(n => n.XMin)
                .HasColumnType("xid")
                .HasColumnName("xmin")
                .IsConcurrencyToken()
                .ValueGeneratedOnAddOrUpdate();
            entity.HasMany(n => n.Items).WithOne().HasForeignKey(i => i.InvoiceId).OnDelete(DeleteBehavior.Cascade);
            // Items agora é IReadOnlyCollection<T> sem setter público; o EF precisa acessar
            // o campo privado "_items" diretamente para materializar e rastrear a coleção.
            entity.Navigation(n => n.Items).UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.ToTable("InvoiceItems");
            entity.HasKey(i => i.Id);
            entity.Property(i => i.Id).ValueGeneratedNever();
            entity.Property(i => i.ProductDescription).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Quantity).IsRequired();
        });

        modelBuilder.Entity<InvoicePrintLog>(entity =>
        {
            entity.ToTable("InvoicePrintLogs");
            entity.HasKey(l => l.Id);
            entity.Property(l => l.Id).ValueGeneratedNever();
            entity.Property(l => l.PrintedDate).IsRequired();
            // Sem navegação de coleção no agregado Invoice: log é lido separadamente (tela de Impressões),
            // não faz parte do ciclo de vida do agregado em si.
            entity.HasOne<Invoice>().WithMany().HasForeignKey(l => l.InvoiceId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
