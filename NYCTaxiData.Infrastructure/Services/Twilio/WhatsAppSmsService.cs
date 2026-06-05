using Microsoft.Extensions.Configuration;
using NYCTaxiData.Application.Common.Interfaces.Services;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace NYCTaxiData.Infrastructure.Services.Twilio;

public class WhatsAppSmsService : ISmsService
{
    private readonly IConfiguration _configuration;

    public WhatsAppSmsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
        if (env == "Development")
        {
            Console.WriteLine($"[DEV OTP] {phoneNumber}: {message}");
            return true;
        }

        try
        {
            var accountSid = _configuration["Twilio:AccountSid"];
            var authToken = _configuration["Twilio:AuthToken"];
            var fromNumber = _configuration["Twilio:PhoneNumber"];

            TwilioClient.Init(accountSid, authToken);

            // تنسيق الرقم
            var formattedPhone = phoneNumber.Trim()
                .Replace(" ", "")
                .Replace("+", "");

            if (formattedPhone.StartsWith("0"))
                formattedPhone = "20" + formattedPhone.Substring(1);
            else if (!formattedPhone.StartsWith("20"))
                formattedPhone = "20" + formattedPhone;

            var msg = await MessageResource.CreateAsync(
                body: message,
                from: new PhoneNumber(fromNumber),
                to: new PhoneNumber($"+{formattedPhone}")
            );

            Console.WriteLine($"✅ SMS sent: {msg.Sid}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ SMS Exception: {ex.Message}");
            return false;
        }
    }

    public Task<string> GetSmsStatusAsync(string messageId)
        => Task.FromResult("Delivered");
}