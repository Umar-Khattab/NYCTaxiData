namespace NYCTaxiData.Application.Common.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateToken(string phoneNumber, string role, string fullName);
}
