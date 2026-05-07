using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.Interfaces
{
    public interface IDailyAggregationService
    {
        Task AggregateAsync(DateTime date, CancellationToken cancellationToken = default);
    }
}
