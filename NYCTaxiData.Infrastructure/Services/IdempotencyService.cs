using Microsoft.Extensions.Caching.Distributed;
using NYCTaxiData.Application.Common.Interfaces;

namespace NYCTaxiData.Infrastructure.Services
{
    public class IdempotencyService : IIdempotencyService
    {
        private readonly IDistributedCache _cache;
        private static readonly TimeSpan DefaultExpiration = TimeSpan.FromHours(24);

        public IdempotencyService(IDistributedCache cache)
        {
            _cache = cache;
        }

        public async Task<string?> GetCachedResponseAsync(string key, CancellationToken cancellationToken)
        {
            return await _cache.GetStringAsync(key, cancellationToken);
        }

        public async Task<bool> IsProcessingAsync(string key, CancellationToken cancellationToken)
        {
            var status = await _cache.GetStringAsync($"processing:{key}", cancellationToken);
            return status != null;
        }

        public async Task MarkAsProcessingAsync(string key, CancellationToken cancellationToken)
        { 
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) };
            await _cache.SetStringAsync($"processing:{key}", "true", options, cancellationToken);
        }
         
        public async Task StoreCachedResponseAsync(string key, string response, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? DefaultExpiration
            };
            await _cache.SetStringAsync(key, response, options, cancellationToken);
        }

        public async Task ClearProcessingMarkerAsync(string key, CancellationToken cancellationToken)
        {
            await _cache.RemoveAsync($"processing:{key}", cancellationToken);
        }

        public async Task RemoveCachedResponseAsync(string key, CancellationToken cancellationToken = default)
        {
            await _cache.RemoveAsync(key, cancellationToken);
        }
    }
}