namespace DirectoryService.Application;

public class HybridCacheOptions
{
    public TimeSpan Expiration { get; set; } = TimeSpan.FromHours(24);

    public TimeSpan LocalCacheExpiration { get; set; } = TimeSpan.FromMinutes(5);
}