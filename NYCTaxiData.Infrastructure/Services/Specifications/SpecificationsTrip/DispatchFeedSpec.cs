using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsTrip
{
    public class DispatchFeedSpec : BaseSpecification<Driver>
    {
        public DispatchFeedSpec(int limit)
        {
            AddCriteria(d =>
                d.Status == CurrentStatus.Available ||
                d.Status == CurrentStatus.On_Trip);

            AddOrderBy(d => d.Fullname!);
            ApplyPaging(0, limit);
        }
    }
}
