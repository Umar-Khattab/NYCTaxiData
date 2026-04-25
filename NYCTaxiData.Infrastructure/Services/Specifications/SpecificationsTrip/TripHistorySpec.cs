using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Specifications.Trips
{
    public class TripHistorySpec : BaseSpecification<Trip>
    {
        // Constructor واحد عبقري بيقوم بالمهمتين
        public TripHistorySpec(Guid? driverId, int page, int limit)
            : base(t => !driverId.HasValue || t.DriverId == driverId.Value)
        {
            // الـ Includes ثابتة في الحالتين عشان الداتا تطلع كاملة
            AddInclude(t => t.Driver!);
            AddInclude(t => t.PickupLocation!);
            AddInclude(t => t.DropoffLocation!);

            AddOrderByDescending(t => t.StartedAt!); 
            // تطبيق الـ Pagination
            ApplyPaging((page - 1) * limit, limit);
            AddInclude(t => t.PickupLocation!);
            AddInclude(t => t.DropoffLocation!);

        }
    }
}
