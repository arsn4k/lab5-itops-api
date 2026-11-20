using Lab5.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Lab5.API.Startup;

/// <summary>
/// Готує базу даних під час запуску застосунку (застосовує міграції).
/// Працює як IHostedService, тому не запускається під час `dotnet ef`.
/// Вимикається налаштуванням Database:ApplyMigrationsOnStartup = false.
/// </summary>
public sealed class DatabaseInitializer : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<DatabaseInitializer> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_configuration.GetValue("Database:ApplyMigrationsOnStartup", true))
        {
            _logger.LogInformation("Автоматичне застосування міграцій вимкнено");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<Lab5DbContext>();

        try
        {
            if (db.Database.GetMigrations().Any())
            {
                await db.Database.MigrateAsync(cancellationToken);
                _logger.LogInformation("Міграції бази даних застосовано");
            }
            else
            {
                // Міграцій ще не створено (див. README): створюємо схему напряму.
                await db.Database.EnsureCreatedAsync(cancellationToken);
                _logger.LogWarning("Міграцій не знайдено, схему створено через EnsureCreated");
            }
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Не вдалося підготувати базу даних");
            throw;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
