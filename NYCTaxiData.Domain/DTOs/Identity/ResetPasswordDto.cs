using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NYCTaxiData.Domain.DTOs.Identity
{
    public class ResetPasswordDto
    {
        [Required]
        public string ResetToken { get; set; }

        [Required]

        public string NewPassword { get; set; }
    }
}
