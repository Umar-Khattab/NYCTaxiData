using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Identity
{
    public class ManagerProfileDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }  
        public string EmployeeId { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }  
    }
}
