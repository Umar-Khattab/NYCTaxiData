using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Identity
{
    public class DriverListDto
    {
        public string DriverId { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PlateNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }
}
