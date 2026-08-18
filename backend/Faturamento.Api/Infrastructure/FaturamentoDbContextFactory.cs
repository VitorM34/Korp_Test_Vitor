using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Faturamento.Api.Infrastructure;

public class FaturamentoDbContextFactory : IDesignTimeDbContextFactory<FaturamentoDbContext>
{
    public FaturamentoDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<FaturamentoDbContext>()
            .UseNpgsql("Host=localhost;Database=faturamento;Username=postgres;Password=postgres")
            .Options;
        return new FaturamentoDbContext(options);
    }
}
