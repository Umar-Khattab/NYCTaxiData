using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Auth.Commands.Login;
using NYCTaxiData.Application.Common.Plumping; 
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Services;
using NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth;

namespace NYCTaxiData.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler(IUnitOfWork _uow, JwtTokenService _jwt, IMapper _mapper)
        : IRequestHandler<LoginCommand, Result<UserResultDto>>  
    {
        public async Task<Result<UserResultDto>> Handle(LoginCommand request, CancellationToken cancellationToken) 
        {
            var spec = new UserForLoginSpec(request.PhoneNumber);
            var user = await _uow.Users.GetBySpecAsync(spec, cancellationToken);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash ?? ""))
            { 
                return Result<UserResultDto>.Failure("Invalid phone number or password");
            }

            var role = user.Driver != null ? "Driver"
                     : user.Manager != null ? "Manager"
                     : "User";

            var fullName = $"{user.FirstName} {user.LastName}";
            var token = _jwt.GenerateToken(user.PhoneNumber, role, fullName);

            var resultDto = _mapper.Map<UserResultDto>(user);
            resultDto.Token = token;
            resultDto.IsSuccess = true;
            resultDto.Role = role;
            resultDto.FullName = fullName;
             
            return Result<UserResultDto>.Success(resultDto);
        }
    }
}