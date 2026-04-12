using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Identity
{
    public class SendOtpDto
    { 
        [Phone]  
        public string PhoneNumber { get; set; }
    }
}
