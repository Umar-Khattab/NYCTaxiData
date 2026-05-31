using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Interfaces.Services;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Domain.Specifications.Users;
using NYCTaxiData.Infrastructure.Services; 

namespace NYCTaxiData.Application.Auth.Queries.GetProfile
{
    public class GetProfileQueryHandler(IUnitOfWork _uow, IMapper _mapper, IJwtTokenService _jwt)
        : IRequestHandler<GetProfileQuery, UserResultDto>
    {
        public async Task<UserResultDto> Handle(GetProfileQuery request, CancellationToken cancellationToken)
        { 
            var spec = new UserForProfileSpec(request.PhoneNumber);
            var user = await _uow.Users.GetBySpecAsync(spec);
             
            if (user == null)
            {
                return new UserResultDto
                {
                    IsSuccess = false,
                    Message = "User not found"
                };
            }
             
            var role = user.Driver != null ? "Driver"
                     : user.Manager != null ? "Manager"
                     : "User";
             
            var result = _mapper.Map<UserResultDto>(user);
            var token = _jwt.GenerateToken(user.Id, user.PhoneNumber, role, $"{user.FirstName} {user.LastName}");
            result.IsSuccess = true;
            result.Role = role;
            result.FullName = $"{user.FirstName} {user.LastName}";
            result.Message = "Profile data retrieved successfully";
            result.Token = token;
            return result;
        }
    }
}
