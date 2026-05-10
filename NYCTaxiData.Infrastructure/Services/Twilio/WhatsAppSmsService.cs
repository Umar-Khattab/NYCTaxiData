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
            var instanceId = "91849";
            var apiToken = "V4ltwCVdMJf8BavnIpng7EKEdxmd1Ip0NBnXg6HQ4df49270";

            var url = $"https://waapi.app/api/v1/instances/{instanceId}/client/action/send-message";

            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiToken);
             
            var formattedPhone = phoneNumber.Trim().Replace("+", "").Replace(" ", "");
             
            if (formattedPhone.StartsWith("0"))
            {
                formattedPhone = "20" + formattedPhone.Substring(1);
            }
            else if (!formattedPhone.StartsWith("20"))
            {
                formattedPhone = "20" + formattedPhone;
            }

            var payload = new
            {
                chatId = $"{formattedPhone}@c.us",  
                message = message
            };

            var response = await client.PostAsJsonAsync(url, payload);

            if (response.IsSuccessStatusCode) return true;
             
            var errorBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"❌ WaAPI Error Details: {errorBody}");

            return false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ SMS Exception: {ex.Message}");
            return false;
        }
    }
    public Task<string> GetSmsStatusAsync(string messageId) => Task.FromResult("Delivered");

}
