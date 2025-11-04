namespace HttpStatusCodeTeacher.Services;

/// <summary>
/// Factory for creating cache service instances based on configuration
/// </summary>
public class CacheServiceFactory(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<CacheServiceFactory> logger)
{
    public ICacheService GetCacheService()
    {
        var cacheType = configuration["Cache:Type"]?.ToLower() ?? "none";

        logger.LogInformation("Creating cache service for type: {CacheType}", cacheType);

        return cacheType switch
        {
            "redis" => serviceProvider.GetRequiredService<RedisCacheService>(),
            "memory" or "inmemory" => serviceProvider.GetRequiredService<InMemoryCacheService>(),
            "none" => serviceProvider.GetRequiredService<NoCacheService>(),
            _ => throw new InvalidOperationException($"Unsupported cache type: {cacheType}. Use 'redis', 'memory', or 'none'.")
        };
    }
}
