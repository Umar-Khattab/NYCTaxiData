using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Tracking
{
    public class AiDispatchOrderDto
    {
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public string DriverPhone { get; set; } = string.Empty;
        public double PickupLatitude { get; set; }
        public double PickupLongitude { get; set; }
        public string PickupZoneName { get; set; } = string.Empty;
        public double DropoffLatitude { get; set; }
        public double DropoffLongitude { get; set; }
        public string DropoffZoneName { get; set; } = string.Empty;
        public decimal EstimatedFare { get; set; }
        public int EstimatedMinutes { get; set; }
        public string Priority { get; set; } = "NORMAL";
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public int ExpiresInSeconds { get; set; } = 30; // السائق عنده 30 ثانية يقبل
    }
}
