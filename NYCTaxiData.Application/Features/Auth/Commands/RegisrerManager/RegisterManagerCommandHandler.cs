using AutoMapper;
using MediatR;
using NYCTaxiData.Application.Common.Interfaces.Services;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Identity;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces; 

namespace NYCTaxiData.Application.Auth.Commands.RegisterManager
{
    public class RegisterManagerCommandHandler(IUnitOfWork _uow, IMapper _mapper, IJwtTokenService _jwt)
        : IRequestHandler<RegisterManagerCommand, Result<UserResultDto>>
    {
        public async Task<Result<UserResultDto>> Handle(RegisterManagerCommand request, CancellationToken ct)
        {
            // 1. ÇáÊÃßÏ ãä ÚÏã ÇáÊßÑÇÑ (ÎÇÑÌ ÇáÜ Transaction)
            if (await _uow.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
                return Result.Failure<UserResultDto>("Phone number already exists", "Conflict");

            if (await _uow.Managers.AnyAsync(m => m.Employeeid == request.EmployeeId))
                return Result.Failure<UserResultDto>("Employee ID already exists", "Conflict");

            return await _uow.ExecuteInTransactionAsync(async (transactionToken) =>
            {
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                // 2. ÅäÔÇÁ ÇáíæÒÑ "ÈÏæä" æÖÚ ID íÏæíÇð
                var user = new User1
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PhoneNumber = request.PhoneNumber,
                    PasswordHash = passwordHash,
                    Userrole = "Manager", 
                    Manager = new Manager
                    {
                        Employeeid = request.EmployeeId,
                        Department = request.Department
                    }
                };
                 
                await _uow.Users.AddAsync(user);
                 
                await _uow.SaveChangesAsync(transactionToken);
                 
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