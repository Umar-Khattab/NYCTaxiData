using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.DTOs.Identity
{
    public class DriverListDto
    {
        public string DriverId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PlateNumber { get; set; }
        public string Status { get; set; }  
    }
}
