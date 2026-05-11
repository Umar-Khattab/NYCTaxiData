using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Infrastructure.Services.Specifications;
using System;
using System.Collections.Generic;
using System.Text; 
namespace NYCTaxiData.Domain.Specifications.Trips
{
    public class DispatchFeedSpec : BaseSpecification<Driver>
    {
        public DispatchFeedSpec(int limit)
        {
            AddCriteria(d =>
                d.Status == CurrentStatus.Available ||
                d.Status == CurrentStatus.On_Trip);

            AddOrderBy(d => d.FullName!);
            ApplyPaging(0, limit);
        }
    }
}
