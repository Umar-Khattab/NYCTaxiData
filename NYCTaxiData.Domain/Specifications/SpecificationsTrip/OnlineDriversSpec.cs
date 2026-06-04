using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Domain.Interfaces.Specifications; 
using System;
using System.Collections.Generic;
using System.Text;
using NYCTaxiData.Infrastructure.Services.Specifications;
namespace NYCTaxiData.Domain.Specifications.Trips
{
    public class OnlineDriversSpec : BaseSpecification<Driver>
    {
        public OnlineDriversSpec(int page, int limit)
            : base(d => d.Status == CurrentStatus.On_Trip.ToString())
        {
            AddOrderByDescending(d => d.Rating!);

            ApplyPaging((page - 1) * limit, limit);
        }
    }
}
