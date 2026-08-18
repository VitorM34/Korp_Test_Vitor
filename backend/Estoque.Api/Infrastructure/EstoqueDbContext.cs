using Estoque.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Api.Infrastructure;

public class EstoqueDbContext(DbContextOptions<EstoqueDbContext> options) : DbContext(options)
{
    public DbSet<Produto> Produtos => Set<Produto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Codigo).IsRequired().HasMaxLength(50);
            entity.HasIndex(p => p.Codigo).IsUnique();
            entity.Property(p => p.Descricao).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Saldo).IsRequired();
            entity.Property(p => p.RowVersion).IsRowVersion().IsConcurrencyToken();
        });
    }
}
