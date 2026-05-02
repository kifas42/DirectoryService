using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class CleanupInactiveRecordsBackgroundService : BackgroundService
{
    private readonly ILogger<CleanupInactiveRecordsBackgroundService> _logger;
    private readonly CleanupInactiveRecordsOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;

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
        using IServiceScope scope = _scopeFactory.CreateScope();
        CleanupInactiveRecordsService cleanupService =
            scope.ServiceProvider.GetRequiredService<CleanupInactiveRecordsService>();

        TimeSpan time = CalculateNextRun();
        _logger.LogDebug("Hello. Next run is {Time}", time);

        await Task.Delay(time, stoppingToken);
        if (stoppingToken.IsCancellationRequested)
        {
            return;
        }

        TimeSpan period = TimeSpan.FromDays(_options.FrequencyDays);
        if (period <= TimeSpan.Zero)
        {
            period = TimeSpan.FromSeconds(1);
            _logger.LogWarning("FrequencyDays <= 0. Установлен тестовый интервал 1 сек.");
        }

        using PeriodicTimer timer = new(period);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await cleanupService.RunCleanupAsync(stoppingToken);
        }
    }

    private TimeSpan CalculateNextRun()
    {
        DateTime now = DateTime.UtcNow;
        DateTime next = now.Date.Add(_options.TriggerTime.ToTimeSpan());
        if (next <= now)
        {
            next = next.AddDays(1);
        }

        return next - now;
    }
}