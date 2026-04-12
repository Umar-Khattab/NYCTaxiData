using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.DTOs.Identity; 
using NYCTaxiData.Infrastructure.Data.Contexts;

namespace NYCTaxiData.Application.Auth.Queries.GetProfile;

public class GetProfileQueryHandler(TaxiDbContext _context)
	: IRequestHandler<GetProfileQuery, UserResultDto>
{
	public async Task<UserResultDto> Handle(
		GetProfileQuery request, CancellationToken cancellationToken)
	{
		var user = await _context.Users1
	  .FirstOrDefaultAsync(u => u.Phonenumber == request.PhoneNumber, cancellationToken);

		if (user is null)
			return new UserResultDto { IsSuccess = false };

		return new UserResultDto
		{
			IsSuccess = true,
			FullName = $"{user.Firstname} {user.Lastname}",
			Role = user.Role.ToString(),
			Message = "Profile data retrieved successfully"
		};
	}
}