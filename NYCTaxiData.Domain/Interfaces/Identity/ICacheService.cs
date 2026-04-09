using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Common.Interfaces.Identity
{
    public interface ICacheService
    {
        Task<string?> GetAsync(string key);
        Task SetAsync(string key, string value, TimeSpan expiry);
        Task RemoveAsync(string key);
    }

}
