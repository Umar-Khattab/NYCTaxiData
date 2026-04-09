using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NYCTaxiData.Domain.DTOs.Identity
{
    public class ForgetPasswordDto
    {
        [Required(ErrorMessage = "Phone number is required.")]
        public string PhoneNumber { get; set; }
    }
}
