using Microsoft.Extensions.Configuration;
using NYCTaxiData.Application.Common.Interfaces.Identity;
using System.Net.Http.Json;

public class WhatsAppSmsService : ISmsService
{
    private readonly IConfiguration _configuration;

    public WhatsAppSmsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        try
        {
            using var client = new HttpClient();

            var instanceId = _configuration["WhatsApp:InstanceId"];
            var token = _configuration["WhatsApp:Token"];
            var url = $"https://api.ultramsg.com/{instanceId}/messages/chat";

            // ✅ تنظيف الرقم: شيل أي علامة + أو مسافات
            var formattedPhone = phoneNumber.Trim().Replace("+", "");

            // ✅ لو الرقم مصري وبيبدأ بـ 01، ضيف كود الدولة 20
            if (formattedPhone.StartsWith("01"))
                formattedPhone = "20" + formattedPhone;
            // ✅ لو الرقم بيبدأ بـ 1 بس (مثلاً 111...)، ضيف 20
            else if (formattedPhone.StartsWith("1"))
                formattedPhone = "20" + formattedPhone;

            var payload = new
            {
                token = token,
                to = formattedPhone,
                body = message
            };

            var response = await client.PostAsJsonAsync(url, payload);

            // لو حبيت تشوف الـ Error اللي راجع في الـ Debugging
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"WhatsApp Error Details: {error}");
            }

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"WhatsApp Exception: {ex.Message}");
            return false;
        }
    }

    public Task<string> GetSmsStatusAsync(string messageId) => Task.FromResult("Delivered");

}
