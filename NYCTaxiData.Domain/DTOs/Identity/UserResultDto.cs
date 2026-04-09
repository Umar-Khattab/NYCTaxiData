using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.DTOs.Identity
{
    public class UserResultDto
    {
        public bool IsSuccess { get; set; } 
        public string? Token { get; set; }
        public string? Message { get; set; }
        public string? Role { get; set; }  
        public string? FullName { get; set; }
    }
}
