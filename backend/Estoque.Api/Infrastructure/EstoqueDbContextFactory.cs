using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Estoque.Api.Infrastructure;

public class EstoqueDbContextFactory : IDesignTimeDbContextFactory<EstoqueDbContext>
{
    public EstoqueDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<EstoqueDbContext>()
            .UseNpgsql("Host=localhost;Database=estoque;Username=postgres;Password=postgres")
            .Options;
        return new EstoqueDbContext(options);
    }
}
