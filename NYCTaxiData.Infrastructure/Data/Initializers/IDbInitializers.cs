using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks; 
using NYCTaxiData.Infrastructure.Data; // Assuming DbContext namespace; adjust as needed

namespace NYCTaxiData.Infrastructure.Data
{
    public interface IDbInitializer
    {
        Task InitializeAsync();
    }
}