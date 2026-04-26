namespace NYCTaxiData.Application.Common.Interfaces.Services;

public interface ISmsService
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
    Task<string> GetSmsStatusAsync(string messageId);
}
