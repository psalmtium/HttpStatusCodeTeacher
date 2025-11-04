using StackExchange.Redis;
using Microsoft.Extensions.Caching.Memory;

namespace HttpStatusCodeTeacher.Services;

/// <summary>
/// Interface for caching data
/// </summary>
public interface ICacheService
{
    Task<string?> GetCacheAsync(string key);
    Task SetCacheAsync(string key, string value, TimeSpan? expiration = null);
}

/// <summary>
/// Redis-based cache service
/// </summary>
public class RedisCacheService : ICacheService
{
    private readonly IConnectionMultiplexer? _redis;
    private readonly IDatabase? _database;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConfiguration configuration, ILogger<RedisCacheService> logger)
    {
        _logger = logger;
        var redisUrl = configuration["Redis:ConnectionString"] ?? "localhost:6379";

        try
        {
            _redis = ConnectionMultiplexer.Connect(redisUrl);
            _database = _redis.GetDatabase();
            _logger.LogInformation("Connected to Redis successfully at {RedisUrl}", redisUrl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis connection failed: {Message}", ex.Message);
            _redis = null;
            _database = null;
        }
    }

    public async Task<string?> GetCacheAsync(string key)
    {
        if (_database == null)
            return null;

        try
        {
            var value = await _database.StringGetAsync(key);
            return value.HasValue ? value.ToString() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache for key: {Key}", key);
            return null;
        }
    }

    public async Task SetCacheAsync(string key, string value, TimeSpan? expiration = null)
    {
        if (_database == null)
            return;

        try
        {
            var expiryTime = expiration ?? TimeSpan.FromHours(1);
            await _database.StringSetAsync(key, value, expiryTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {Key}", key);
        }
    }
}

/// <summary>
/// In-memory cache service using MemoryCache
/// </summary>
public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;
    private readonly ILogger<InMemoryCacheService> _logger;

    public InMemoryCacheService(IMemoryCache cache, ILogger<InMemoryCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
        _logger.LogInformation("Using in-memory caching");
    }

    public Task<string?> GetCacheAsync(string key)
    {
        try
        {
            var value = _cache.Get<string>(key);
            return Task.FromResult(value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache for key: {Key}", key);
            return Task.FromResult<string?>(null);
        }
    }

    public Task SetCacheAsync(string key, string value, TimeSpan? expiration = null)
    {
        try
        {
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1)
            };
            _cache.Set(key, value, cacheOptions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cache for key: {Key}", key);
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// No-op cache service that doesn't cache anything
/// </summary>
public class NoCacheService : ICacheService
{
    private readonly ILogger<NoCacheService> _logger;

    public NoCacheService(ILogger<NoCacheService> logger)
    {
        _logger = logger;
        _logger.LogInformation("Caching is disabled");
    }

    public Task<string?> GetCacheAsync(string key)
    {
        return Task.FromResult<string?>(null);
    }

    public Task SetCacheAsync(string key, string value, TimeSpan? expiration = null)
    {
        return Task.CompletedTask;
    }
}
