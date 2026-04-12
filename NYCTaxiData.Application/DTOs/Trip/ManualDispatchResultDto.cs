using System;
using System.Collections.Generic;
using System.Text;
namespace NYCTaxiData.Application.DTOs.Trip
{
    public class ManualDispatchResultDto
    {
        public string DispatchId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string DriverName { get; set; } = string.Empty;
        public string TargetZone { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
    }
}
