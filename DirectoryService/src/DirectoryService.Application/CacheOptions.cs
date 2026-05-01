namespace DirectoryService.Application;

public class CacheOptions
{
    public TimeSpan Expiration { get; init; } = TimeSpan.FromHours(24);

    public TimeSpan LocalCacheExpiration { get; init; } = TimeSpan.FromMinutes(5);
}

public class CacheConstants
{
    public const string SECTION_NAME = "HybridCache";
    public const string REDIS_SECTION = "Redis";
    public const string TOP_DEPARTMENTS_TAG = "top_departments";
    public const string DEPARTMENTS_TAG = "departments";
}