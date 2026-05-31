using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Interfaces.Services;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Domain.Specifications.Drivers;
using NYCTaxiData.Domain.Specifications.Users;
using NYCTaxiData.Infrastructure;
using NYCTaxiData.Infrastructure.Services; 

namespace NYCTaxiData.Application.Auth.Commands.RegisterDriver
{
    public class RegisterDriverCommandHandler(IUnitOfWork _uow, IMapper _mapper, IJwtTokenService _jwt)
      : IRequestHandler<RegisterDriverCommand, Result<UserResultDto>>
    {
        public async Task<Result<UserResultDto>> Handle(RegisterDriverCommand request, CancellationToken cancellationToken)
        {
            // 1. Validation Checks (Specifications)
            if (await _uow.Users.AnyWithSpecAsync(new UserPhoneExistsSpec(request.PhoneNumber), cancellationToken))
                return Result<UserResultDto>.Failure("Phone number already exists");

            if (await _uow.Drivers.AnyWithSpecAsync(new DriverLicenseExistsSpec(request.LicenseNumber), cancellationToken))
                return Result<UserResultDto>.Failure("License number is already registered to another driver");

            if (await _uow.Drivers.AnyWithSpecAsync(new DriverPlateExistsSpec(request.PlateNumber), cancellationToken))
                return Result<UserResultDto>.Failure("Plate number is already registered to another vehicle");

            // 2. Mapping & Preparation
            var user = _mapper.Map<User1>(request);
            var driver = _mapper.Map<Driver>(request);

            // ? «·ŒÿÊ… «·–Â»Ì…:  ‘›Ì— «·»«”Ê—œ Ê„·¡ «·⁄„Êœ «·„ÿ·Ê» ›Ì «·œ« «»Ì“
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // «·—»ÿ «·”Õ—Ì (Navigation Property)
            user.Driver = driver;
            user.Userrole = "Driver";

            // 3. Persistence
            var saveResult = await _uow.ExecuteInTransactionAsync(async ct =>
            {
                await _uow.Users.AddAsync(user);
                // «·‹ EF ÂÌ”Ì› «·ÌÊ“— √Ê·«° Ì«Œœ «·‹ ID° ÌÕÿÂ ··”Ê«ﬁ° ÊÌ”Ì› «·”Ê«ﬁ.. ﬂ·Â ›Ì Transaction Ê«Õœ…
                return await _uow.SaveChangesAsync(ct);
            }, cancellationToken);

            if (saveResult <= 0)
                return Result<UserResultDto>.Failure("Failed to save driver data.");

            // 4. Response Generation
            var fullName = $"{user.FirstName} {user.LastName}";
            var token = _jwt.GenerateToken(user.PhoneNumber, "Driver", fullName);

            return Result<UserResultDto>.Success(new UserResultDto
            {
                IsSuccess = true,
                FullName = fullName,
                Role = "Driver",
                Token = token,
                Message = "Driver registered successfully"
            });
        }
    }
}