namespace NYCTaxiData.Application.Common.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateToken(Guid userId, string phoneNumber, string role, string fullName);
}
