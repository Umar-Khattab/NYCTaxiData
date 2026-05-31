using AutoMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NYCTaxiData.Application.Common.Interfaces.Services; 
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Domain.Specifications.Users;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NYCTaxiData.Application.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler(
        IUnitOfWork _uow,
        IConfiguration _config,
        IJwtTokenService _jwtService,
        IMapper _mapper)
        : IRequestHandler<RefreshTokenCommand, UserResultDto>
    {
        public async Task<UserResultDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        { 
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Secret"] ?? "default-secret-key-min32chars-longer");

            ClaimsPrincipal principal;
            try
            {
                principal = tokenHandler.ValidateToken(request.OldToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _config["Jwt:Issuer"] ?? "NYCTaxiData",
                    ValidateAudience = true,
                    ValidAudience = _config["Jwt:Audience"] ?? "NYCTaxiData",
                    ValidateLifetime = false  
                }, out _);
            }
            catch
            {
                return new UserResultDto { IsSuccess = false, Message = "Invalid token format or signature" };
            }
             
            var userIdString = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
                return new UserResultDto { IsSuccess = false, Message = "Invalid token claims" };
             
            var user = await _uow.Users.GetByIdAsync(userId);

            if (user == null)
                return new UserResultDto { IsSuccess = false, Message = "User associated with this token no longer exists" };
             
            var role = user.Driver != null ? "Driver"
                     : user.Manager != null ? "Manager"
                     : "User";

            var fullName = $"{user.FirstName} {user.LastName}"; 
            var newToken = _jwtService.GenerateToken(user.Id, user.PhoneNumber, role, fullName);
             
            var result = _mapper.Map<UserResultDto>(user); 
            result.IsSuccess = true;
            result.Message = "Token refreshed successfully";
            result.Token = newToken;
            return result;
        }
    }
}