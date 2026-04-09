using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Application.Common.Interfaces.Identity
{
    public interface ISmsService
    {
        Task<bool> SendSmsAsync(string phoneNumber, string message);
        Task<string> GetSmsStatusAsync(string messageId);
    }
}

