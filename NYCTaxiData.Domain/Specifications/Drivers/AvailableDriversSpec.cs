 
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Infrastructure.Services.Specifications;
using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.Specifications.Drivers
{
    public class AvailableDriversSpec : BaseSpecification<Driver>
    {
        public AvailableDriversSpec(int page, int limit)
        {
            AddCriteria(d => d.Status == CurrentStatus.Available.ToString());

            AddInclude(d => d.User!);
            AddOrderBy(d => d.User!.FirstName!);
            ApplyPaging((page - 1) * limit, limit);
        }
    }
}
