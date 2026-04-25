using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsTrip
{
    public class OnlineDriversSpec : BaseSpecification<Driver>
    {
        public OnlineDriversSpec(int page, int limit)
            : base(d => d.Status == CurrentStatus.On_Trip)
        {
            AddOrderByDescending(d => d.Rating!);

            ApplyPaging((page - 1) * limit, limit);
        }
    }
}
