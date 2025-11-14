using JasperFx.Core;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Basket.Api.Data;

public class CachedBasketRepository
    (IBasketRepository repository, IDistributedCache cache, IMemoryCache memoryCache, ICacheOptionsFactory cacheOptionsFactory)
    : IBasketRepository
{
    public async Task<ShoppingCart> GetBasket(string userName, CancellationToken cancellationToken = default)
    {
        if (memoryCache.TryGetValue(userName, out string? memoryCacheBasket)
     && !string.IsNullOrEmpty(memoryCacheBasket))
        {
            return JsonSerializer.Deserialize<ShoppingCart>(memoryCacheBasket)!;
        }


        var cachedBasket = await cache.GetStringAsync(userName, cancellationToken);
        if (!string.IsNullOrEmpty(cachedBasket))
            return JsonSerializer.Deserialize<ShoppingCart>(cachedBasket)!;

        var basket = await repository.GetBasket(userName, cancellationToken);
        await cache.SetStringAsync(userName, JsonSerializer.Serialize(basket), cacheOptionsFactory.CreateDistributedOptions(), cancellationToken);
        memoryCache.Set(basket.UserName, JsonSerializer.Serialize(basket), cacheOptionsFactory.CreateMemoryOptions());
        return basket;
    }

    public async Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        await repository.StoreBasket(basket, cancellationToken);

        await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket), cacheOptionsFactory.CreateDistributedOptions(), cancellationToken);

        memoryCache.Set(basket.UserName,JsonSerializer.Serialize(basket),cacheOptionsFactory.CreateMemoryOptions());

        return basket;
    }

    public async Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
    {
        await repository.DeleteBasket(userName, cancellationToken);

        await cache.RemoveAsync(userName, cancellationToken);
        memoryCache.Remove(userName);

        return true;
    }
}