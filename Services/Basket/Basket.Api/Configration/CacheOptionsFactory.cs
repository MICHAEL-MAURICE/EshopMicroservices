using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;

namespace Basket.Api.Configration;

public interface ICacheOptionsFactory
{
    MemoryCacheEntryOptions CreateMemoryOptions();
    DistributedCacheEntryOptions CreateDistributedOptions();
}

public class CacheOptionsFactory : ICacheOptionsFactory
{
    private readonly IConfiguration _configuration;

    private const string MemoryExpirationKey = "CacheSettings:MemoryAbsoluteExpirationSeconds";
    private const string DistributedExpirationKey = "CacheSettings:DistributedAbsoluteExpirationMinutes";

    public CacheOptionsFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public MemoryCacheEntryOptions CreateMemoryOptions()
    {
        var seconds = _configuration.GetValue<int?>(MemoryExpirationKey) ?? 30;

        return new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromSeconds(seconds));
    }

    public DistributedCacheEntryOptions CreateDistributedOptions()
    {
        var minutes = _configuration.GetValue<int?>(DistributedExpirationKey) ?? 30;

        return new DistributedCacheEntryOptions()
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(minutes));
    }
}