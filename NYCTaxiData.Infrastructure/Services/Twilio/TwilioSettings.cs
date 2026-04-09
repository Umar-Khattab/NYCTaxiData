using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services.Twilio
{
    public class TwilioSettings
    {
        public string AccountSid { get; set; }
        public string AuthToken { get; set; }
        public string PhoneNumber { get; set; }
    }
}
