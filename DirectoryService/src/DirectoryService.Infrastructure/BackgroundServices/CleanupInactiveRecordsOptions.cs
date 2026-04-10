using System.ComponentModel.DataAnnotations;

namespace DirectoryService.Infrastructure.BackgroundServices;

public class CleanupInactiveRecordsOptions
{
    [Required]
    [RegularExpression(@"^([01]\d|2[0-3]):[0-5]\d$")]
    public TimeOnly TriggerTime { get; set; } = new(2, 0);

    [Required]
    [Range(1, 365)]
    public int FrequencyDays { get; set; } = 30;

    [Required]
    [Range(100, 10000)]
    public int BatchSize { get; set; } = 1000;

    [Required]
    [Range(0, 3650)]
    public int RetentionDays { get; set; } = 30;
}