using Microsoft.AspNetCore.Http;
using NYCTaxiData.Domain.Enums;
using System.Security.Claims;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Application.Common.Interfaces;

namespace NYCTaxiData.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        // 1️⃣ الـ UserId كـ Guid (للاستخدام الداخلي في الـ Entities)
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

        public string? Role => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;

        public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value;

        public string? PhoneNumber => _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.MobilePhone)?.Value
                                     ?? _httpContextAccessor.HttpContext?.User?.FindFirst("phone")?.Value;

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}