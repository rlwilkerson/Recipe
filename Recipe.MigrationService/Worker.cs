using Microsoft.EntityFrameworkCore;
using Recipe.Web.Data;

namespace Recipe.MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await RunMigrationAsync(stoppingToken);
            await SeedDataAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Migration/seeding failed");
            throw;
        }
        finally
        {
            hostApplicationLifetime.StopApplication();
        }
    }

    private async Task RunMigrationAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        logger.LogInformation("Applying EF Core migrations...");
        await dbContext.Database.MigrateAsync(cancellationToken);
        logger.LogInformation("Migrations applied.");
    }

    private async Task SeedDataAsync(CancellationToken cancellationToken)
    {
        await using var scope = serviceProvider.CreateAsyncScope();
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
