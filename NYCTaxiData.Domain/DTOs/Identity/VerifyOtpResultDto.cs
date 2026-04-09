using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.DTOs.Identity
{
    public class VerifyOtpResultDto : UserResultDto
    {
        public string? ResetToken { get; set; }
    }

}
