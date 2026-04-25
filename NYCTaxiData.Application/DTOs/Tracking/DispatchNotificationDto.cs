using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Tracking
{
    public class DispatchNotificationDto
    {
        public string DispatchId { get; set; } = Guid.NewGuid().ToString();
        public string DriverPhone { get; set; } = string.Empty;
        public string TargetZoneId { get; set; } = string.Empty;
        public string TargetZoneName { get; set; } = string.Empty;
        public string Priority { get; set; } = "NORMAL";
        public string Message { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
        public int? EstimatedMinutes { get; set; }
    }
}
