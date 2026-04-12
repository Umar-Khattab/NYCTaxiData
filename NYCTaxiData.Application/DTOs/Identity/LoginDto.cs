using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Identity
{
    public class LoginDto
    {
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
    }
}
