using NYCTaxiData.Domain.Interfaces.ConcurrentEntity;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.ConcurrentEntity
{
    public partial class TripConcurrent : IConcurrentEntity
    {
        public byte[]? RowVersion { get; set; }
    }
}
