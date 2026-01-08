using System.Text;
using System.Text.Json;
using ConsolidadoDiario.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ConsolidadoDiario.Infrastructure.Cache;

public class RedisCacheService : ICacheService, IDisposable
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IDatabase _database;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(IConfiguration configuration, ILogger<RedisCacheService> logger)
    {
        _logger = logger;
        var connectionString = configuration["Redis:ConnectionString"] ?? "localhost:6379";
        
        try
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _database = _redis.GetDatabase();
            _logger.LogInformation("Redis connection established");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish Redis connection. Cache will not be available.");
            throw;
        }
    }

    public async Task<T?> ObterAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var value = await _database.StringGetAsync(key);
            if (!value.HasValue)
                return null;

            var json = value.ToString();
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting value from cache: {Key}", key);
            return null;
        }
    }

    public async Task DefinirAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            if (expiration.HasValue)
            {
                await _database.StringSetAsync(key, json, expiration.Value);
            }
            else
            {
                await _database.StringSetAsync(key, json);
            }
            _logger.LogDebug("Value cached: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting value to cache: {Key}", key);
            // Não lançar exceção para não bloquear a operação principal
        }
    }

    public async Task RemoverAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _database.KeyDeleteAsync(key);
            _logger.LogDebug("Cache key removed: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing cache key: {Key}", key);
        }
    }

    public void Dispose()
    {
        _redis?.Close();
        _redis?.Dispose();
    }
}

