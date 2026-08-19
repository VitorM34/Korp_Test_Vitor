using Inventory.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api;

public sealed class WarmupService(IServiceProvider services, ILogger<WarmupService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(300, stoppingToken);
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
            await db.Products.AnyAsync(stoppingToken);
            logger.LogInformation("Warmup do Inventory concluído.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Warmup falhou (não crítico).");
        }
    }
}
