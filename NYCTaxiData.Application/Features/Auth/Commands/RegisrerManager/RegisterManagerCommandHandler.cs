using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure;
using NYCTaxiData.Infrastructure.Services;
using NYCTaxiData.Infrastructure.Services.Specifications.Managers;  
using NYCTaxiData.Infrastructure.Services.Specifications.SpecificationsAuth;
using Twilio.Jwt.AccessToken;
using Twilio.TwiML.Messaging;

namespace NYCTaxiData.Application.Auth.Commands.RegisterManager
{
    public class RegisterManagerCommandHandler(IUnitOfWork _uow, IMapper _mapper, JwtTokenService _jwt)
        : IRequestHandler<RegisterManagerCommand, Result<UserResultDto>>
    {
        public async Task<Result<UserResultDto>> Handle(RegisterManagerCommand request, CancellationToken ct)
        {
            // 1. التأكد من عدم التكرار (خارج الـ Transaction)
            if (await _uow.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
                return Result.Failure<UserResultDto>("Phone number already exists", "Conflict");

            if (await _uow.Managers.AnyAsync(m => m.Employeeid == request.EmployeeId))
                return Result.Failure<UserResultDto>("Employee ID already exists", "Conflict");

            return await _uow.ExecuteInTransactionAsync(async (transactionToken) =>
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // 2. إنشاء اليوزر "بدون" وضع ID يدوياً
                var user = new User1
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = passwordHash,
                    Userrole = "Manager",
                    // 👈 الربط السحري: بنحط كائن المانجر جوه اليوزر مباشرة
                    Manager = new Manager
                    {
                        Employeeid = request.EmployeeId,
                        Department = request.Department
                    }
                };

                // 3. بنعمل Add لليوزر بس، وهو هيسحب المانجر معاه
                await _uow.Users.AddAsync(user);

                // 4. سيف الكل: الـ EF هيولد الـ ID لليوزر، وياخده يحطه للمانجر، ويسيفهم بالترتيب الصح
                await _uow.SaveChangesAsync(transactionToken);

                // 5. توليد الـ Token بعد ما البيانات اتسيفت وبقى ليها ID حقيقي
                var fullName = $"{user.FirstName} {user.LastName}";
                var token = _jwt.GenerateToken(user.PhoneNumber, "Manager", fullName);

                return Result.Success(new UserResultDto
                {
                    IsSuccess = true,
                    FullName = fullName,
                    Message= "Manager registered successfully",
                    Role = "Manager",
                    Token = token
                });
            }, ct);
        }
    }
}