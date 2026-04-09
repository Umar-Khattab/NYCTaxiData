using NYCTaxiData.Application.Common.Interfaces.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services
{
    public class CacheService : ICacheService
    { 
        public Task<string?> GetAsync(string key)
        { 
            return Task.FromResult<string?>(null);
        }

        public Task SetAsync(string key, string value, TimeSpan expiry)
        { 
            return Task.CompletedTask;
        }

        public Task RemoveAsync(string key)
        {
            return Task.CompletedTask;
        }
    }
}
