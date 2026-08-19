using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Billing.Api.Infrastructure;

public class BillingDbContextFactory : IDesignTimeDbContextFactory<BillingDbContext>
{
    public BillingDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("BillingDb")
            ?? "Host=localhost;Port=5434;Database=billing;Username=postgres;Password=postgres";

        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new BillingDbContext(options);
    }
}
