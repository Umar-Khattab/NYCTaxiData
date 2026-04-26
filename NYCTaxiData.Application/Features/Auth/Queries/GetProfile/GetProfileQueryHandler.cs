using MediatR;
using NYCTaxiData.Application.Common.Interfaces;
using NYCTaxiData.Application.DTOs.Identity; 

namespace NYCTaxiData.Application.Auth.Queries.GetProfile;

public class GetProfileQueryHandler(IApplicationDbContext _context)
	: IRequestHandler<GetProfileQuery, UserResultDto>
{
	public async Task<UserResultDto> Handle(
		GetProfileQuery request, CancellationToken cancellationToken)
	{
      var user = await _context.GetUserByPhoneAsync(request.PhoneNumber, cancellationToken);

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