using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NYCTaxiData.Application.DTOs.Identity
{
    public class ResetPasswordDto
    {
        [Required]
        public string ResetToken { get; set; } = string.Empty;

        [Required]

        public string NewPassword { get; set; } = string.Empty;
    }
}
