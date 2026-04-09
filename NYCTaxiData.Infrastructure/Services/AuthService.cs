using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NYCTaxiData.Application.Common.Interfaces.Identity;
using NYCTaxiData.Domain.DTOs.Identity;
using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure.Data.Contexts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NYCTaxiData.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly TaxiDbContext _context;
    private readonly ICacheService _cacheService;
    private readonly IConfiguration _configuration;
    private readonly ISmsService _smsService;
    private readonly IMapper _mapper;

    public AuthService(
        IUnitOfWork uow,
        TaxiDbContext context,
        ICacheService cacheService,
        IConfiguration configuration,
        ISmsService smsService,
        IMapper mapper)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _smsService = smsService ?? throw new ArgumentNullException(nameof(smsService));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    // ===== Login =====
    public async Task<UserResultDto> LoginAsync(LoginDto loginDto)
    {
        var user = await FindUserByPhoneAsync(loginDto.PhoneNumber);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash ?? ""))
            return new UserResultDto { IsSuccess = false };

        var token = GenerateJwtToken(user.PhoneNumber!, user.Role!, user.FullName!);
        return new UserResultDto
        {
            IsSuccess = true,
            Token = token,
            FullName = user.FullName!,
            Role = user.Role!
        };
    }

    // ===== Register Driver =====
    public async Task<UserResultDto> RegisterDriverAsync(DriverRegisterDto dto)
    {
        var exists = await _uow.Users.AnyAsync(u => u.Phonenumber == dto.PhoneNumber);
        if (exists)
            return new UserResultDto { IsSuccess = false, Message = "Phone number already exists" };

        var userId = Guid.NewGuid();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var now = DateTime.UtcNow;
        var fullName = $"{dto.FirstName} {dto.LastName}";
         
        await _context.Database.ExecuteSqlRawAsync(@"
    INSERT INTO users (id, age, city, createdat, email, firstname, lastname, passwordhash, phonenumber, role, street)
    VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}::user_role, {10})",
            userId, dto.Age, dto.City ?? "N/A", now,
            dto.PhoneNumber + "@taxi.com",
            dto.FirstName, dto.LastName, passwordHash,
            dto.PhoneNumber,
            "Driver", // <--- التعديل هنا (D كابيتال)
            dto.Street ?? "N/A"
        );
         
        var driver = _mapper.Map<Driver>(dto);
        driver.Id = userId;

        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();

        var token = GenerateJwtToken(dto.PhoneNumber, "Driver", fullName);
        return new UserResultDto { IsSuccess = true, Token = token, FullName = fullName, Role = "Driver" };

    }

    // ===== Register Manager =====
    public async Task<UserResultDto> RegisterManagerAsync(ManagerRegisterDto dto)
    {
        var exists = await _uow.Users.AnyAsync(u => u.Phonenumber == dto.PhoneNumber);
        if (exists)
            return new UserResultDto { IsSuccess = false, Message = "Phone number already exists" };

        var userId = Guid.NewGuid();
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var now = DateTime.UtcNow;
         
        await _context.Database.ExecuteSqlRawAsync(@"
    INSERT INTO users (id, age, city, createdat, email, firstname, lastname, passwordhash, phonenumber, role, street)
    VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}::user_role, {10})",
            userId, dto.Age, dto.City ?? "N/A", now,
            dto.PhoneNumber + "@taxi.com",
            dto.FirstName, dto.LastName, passwordHash,
            dto.PhoneNumber,
            "Manager", // <--- التعديل هنا (M كابيتال)
            dto.Street ?? "N/A"
        );
         
        var manager = _mapper.Map<Manager>(dto);
        manager.Id = userId;

        await _uow.Managers.AddAsync(manager);
        await _uow.SaveChangesAsync();

        var fullName = $"{dto.FirstName} {dto.LastName}";
        var token = GenerateJwtToken(dto.PhoneNumber, "Manager", fullName);
        return new UserResultDto { IsSuccess = true, Token = token, FullName = fullName, Role = "Manager" };
    }

    // ===== Send OTP =====
    public async Task<ResultDto> SendOtpAsync(SendOtpDto dto)
    {
        var otp = new Random().Next(100000, 999999).ToString();
        var cacheKey = $"otp:{dto.PhoneNumber}";

        await _cacheService.SetAsync(cacheKey, otp, TimeSpan.FromMinutes(5));

        var smsSent = await _smsService.SendSmsAsync(dto.PhoneNumber, $"Your OTP is: {otp}");
        if (!smsSent)
        {
            await _cacheService.RemoveAsync(cacheKey);
            return new ResultDto { IsSuccess = false, Message = "Failed to send OTP" };
        }

        return new ResultDto { IsSuccess = true, Message = "OTP sent successfully" };
    }

    // ===== Verify OTP =====
    public async Task<VerifyOtpResultDto> VerifyOtpAsync(VerifyOtpDto dto)
    {
        var cachedOtp = await _cacheService.GetAsync($"otp:{dto.PhoneNumber}");
        if (cachedOtp != dto.OtpCode)
            return new VerifyOtpResultDto { IsSuccess = false };

        await _cacheService.RemoveAsync($"otp:{dto.PhoneNumber}");

        var resetToken = Convert.ToBase64String(
            Encoding.UTF8.GetBytes($"{dto.PhoneNumber}:{Guid.NewGuid()}:reset"));
        await _cacheService.SetAsync($"reset:{resetToken}", dto.PhoneNumber, TimeSpan.FromMinutes(15));

        var result = new VerifyOtpResultDto { IsSuccess = true, ResetToken = resetToken };

        var user = await FindUserByPhoneAsync(dto.PhoneNumber);
        if (user != null)
        {
            result.Token = GenerateJwtToken(user.PhoneNumber!, user.Role!, user.FullName!);
            result.FullName = user.FullName!;
            result.Role = user.Role!;
        }

        return result;
    }

    // ===== Reset Password =====
    public async Task<UserResultDto> ResetPasswordAsync(ResetPasswordDto dto)
    {
        // 1. التأكد من صحة التوكن من الـ Cache
        var phoneKey = await _cacheService.GetAsync($"reset:{dto.ResetToken}");
        if (string.IsNullOrEmpty(phoneKey))
            return new UserResultDto { IsSuccess = false, Message = "Invalid or expired reset token." };

        // 2. البحث عن المستخدم برقم الهاتف المسترجع
        var user = await _uow.Users.GetFirstOrDefaultAsync(u => u.Phonenumber == phoneKey);
        if (user == null)
            return new UserResultDto { IsSuccess = false, Message = "User not found." };

        // 3. تحديث الباسورد بعد التشفير
        user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _uow.Users.UpdateAsync(user);
        await _uow.SaveChangesAsync();

        // 4. حذف التوكن من الـ Cache عشان ميتستخدمش تاني (Security Best Practice)
        await _cacheService.RemoveAsync($"reset:{dto.ResetToken}");

        // 5. جلب بيانات المستخدم كاملة لإنشاء الـ Token
        var fullUser = await FindUserByPhoneAsync(phoneKey);

        var jwtToken = GenerateJwtToken(
            fullUser.PhoneNumber ?? phoneKey,
            fullUser.Role ?? "Client",
            fullUser.FullName ?? "Valued User"
        );

        return new UserResultDto
        {
            IsSuccess = true,
            Message = "Password has been reset successfully.",
            Token = jwtToken,
            FullName = fullUser.FullName ?? "User",
            Role = fullUser.Role ?? "Client"
        };
    }

    // ===== Private Helpers =====
    private async Task<UserInfo> FindUserByPhoneAsync(string phoneNumber)
    {
        // 1. هات اليوزر الأساسي
        var user = await _uow.Users.GetFirstOrDefaultAsync(u => u.Phonenumber == phoneNumber);
        if (user == null) return null!;

        var fullName = $"{user.Firstname} {user.Lastname}";

        // 2. تحديد الـ Role بناءً على اللي متسجل في جدول الـ Users أصلاً
        // أو التأكد من الجداول الفرعية
        var role = "User";

        if (await _uow.Drivers.AnyAsync(d => d.Id == user.Id))
            role = "Driver"; // تأكد إنها بتبدأ بـ D كابيتال عشان تطابق الـ Register
        else if (await _uow.Managers.AnyAsync(m => m.Id == user.Id))
            role = "Manager"; // تأكد إنها بتبدأ بـ M كابيتال

        // ملاحظة: لو إنت مسجل الـ Role في جدول الـ Users كـ Enum، 
        // ممكن تجيبه مباشرة: role = user.Role.ToString();

        return new UserInfo(user.Phonenumber, user.Passwordhash, role, fullName);
    }

    private string GenerateJwtToken(string phoneNumber, string role, string fullName)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"] ?? "default-secret-key-min32chars-longer"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, phoneNumber),
            new Claim(ClaimTypes.Role, role),
            new Claim("FullName", fullName)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "NYCTaxiData",
            audience: _configuration["Jwt:Audience"] ?? "NYCTaxiData",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record UserInfo(string PhoneNumber, string PasswordHash, string Role, string FullName);