using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Identity
{
    public class VerifyOtpDto
    {
        [Required]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "OTP code required")]
        public string OtpCode { get; set; }
    }

}
