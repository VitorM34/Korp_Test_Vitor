using Faturamento.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Faturamento.Api.Infrastructure;

public class FaturamentoDbContext(DbContextOptions<FaturamentoDbContext> options) : DbContext(options)
{
    public DbSet<NotaFiscal> NotasFiscais => Set<NotaFiscal>();
    public DbSet<ItemNotaFiscal> Itens => Set<ItemNotaFiscal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<NotaFiscal>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.Numeracao).IsRequired();
            entity.HasIndex(n => n.Numeracao).IsUnique();
            entity.Property(n => n.Status).IsRequired().HasConversion<string>();
            entity.Property(n => n.DataCriacao).IsRequired();
            entity.HasMany(n => n.Itens).WithOne().HasForeignKey(i => i.NotaFiscalId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemNotaFiscal>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.Property(i => i.DescricaoProduto).IsRequired().HasMaxLength(200);
            entity.Property(i => i.Quantidade).IsRequired();
        });
    }
}
