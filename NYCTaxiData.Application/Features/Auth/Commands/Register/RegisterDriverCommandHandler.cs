using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure;
using NYCTaxiData.Infrastructure.Services;
using NYCTaxiData.Infrastructure.Services.Specifications.Drivers;
using NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth;

namespace NYCTaxiData.Application.Auth.Commands.RegisterDriver
{
    public class RegisterDriverCommandHandler(IUnitOfWork _uow, IMapper _mapper, JwtTokenService _jwt)
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

            // ✅ الخطوة الذهبية: تشفير الباسورد وملء العمود المطلوب في الداتابيز
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // الربط السحري (Navigation Property)
            user.Driver = driver;
            user.Userrole = "Driver";

            // 3. Persistence
            var saveResult = await _uow.ExecuteInTransactionAsync(async ct =>
            {
                await _uow.Users.AddAsync(user);
                // الـ EF هيسيف اليوزر أولاً، ياخد الـ ID، يحطه للسواق، ويسيف السواق.. كله في Transaction واحدة
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