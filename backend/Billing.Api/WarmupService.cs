using Billing.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Billing.Api;

public sealed class WarmupService(IServiceProvider services, ILogger<WarmupService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(300, stoppingToken);
        try
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
            await db.Invoices.AnyAsync(stoppingToken);
            logger.LogInformation("Warmup do Billing concluído.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Warmup falhou (não crítico).");
        }
    }
}
