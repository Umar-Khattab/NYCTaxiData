using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.DTOs.Identity
{// Forget Password
    public class ResultDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
