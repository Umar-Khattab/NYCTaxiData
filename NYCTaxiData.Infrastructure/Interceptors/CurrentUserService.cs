using Microsoft.AspNetCore.Http;
using NYCTaxiData.Domain.Interfaces;
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
         
        public string? UserId => _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public string? UserName => _httpContextAccessor.HttpContext?.User?
            .FindFirst("FullName")?.Value;

        public string? Role => _httpContextAccessor.HttpContext?.User?
            .FindFirst(ClaimTypes.Role)?.Value;

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?
            .Identity?.IsAuthenticated ?? false;
    }
}
