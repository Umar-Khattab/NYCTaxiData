using Microsoft.AspNetCore.Http;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Domain.Enums;
using System; 
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
         
        public Guid? UserId => Guid.TryParse(
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            out var userId)
            ? userId
            : null;

        public UserRole? UserRole => Enum.TryParse<UserRole>(
            _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value,
            ignoreCase: true,
            out var role)
            ? role
            : null;

        public string? PhoneNumber => _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?
            .Identity?.IsAuthenticated ?? false;
    }
}
