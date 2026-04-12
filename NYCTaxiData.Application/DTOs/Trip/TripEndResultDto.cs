using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Trip
{
    public class TripEndResultDto
    {
        public int TripId { get; set; }
        public int DurationMinutes { get; set; }
        public decimal BaseFare { get; set; }
        public decimal SurgeMultiplier { get; set; }
        public decimal TotalFare { get; set; }
        public DateTime EndedAt { get; set; }
        public string Status { get; set; } = "Completed";
    }
}
