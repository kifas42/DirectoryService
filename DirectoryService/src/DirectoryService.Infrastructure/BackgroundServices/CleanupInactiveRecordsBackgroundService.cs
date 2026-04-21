using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class CleanupInactiveRecordsBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<CleanupInactiveRecordsBackgroundService> _logger;
    private readonly CleanupInactiveRecordsOptions _options;

    public CleanupInactiveRecordsBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<CleanupInactiveRecordsBackgroundService> logger,
        IOptions<CleanupInactiveRecordsOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var cleanupService = scope.ServiceProvider.GetRequiredService<CleanupInactiveRecordsService>();

        var time = CalculateNextRun();
        _logger.LogDebug("Hello. Next run is {Time}", time);

        await Task.Delay(time, stoppingToken);
        if (stoppingToken.IsCancellationRequested) return;

        var period = TimeSpan.FromDays(_options.FrequencyDays);
        if (period <= TimeSpan.Zero)
        {
            period = TimeSpan.FromSeconds(1);
            _logger.LogWarning("FrequencyDays <= 0. Установлен тестовый интервал 1 сек.");
        }

        using var timer = new PeriodicTimer(period);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await cleanupService.RunCleanupAsync(stoppingToken);
        }
    }

    private TimeSpan CalculateNextRun()
    {
        var now = DateTime.UtcNow;
        var next = now.Date.Add(_options.TriggerTime.ToTimeSpan());
        if (next <= now) next = next.AddDays(1);
        return next - now;
    }
}