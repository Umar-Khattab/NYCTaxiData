using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.DTOs.Identity
{
    public class ManagerRegisterDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int Age { get; set; }  
        public string? City { get; set; }  
        public string? Street { get; set; }
        public string PhoneNumber { get; set; }
        public string EmployeeId { get; set; }
        public string Department { get; set; }
    }
}
