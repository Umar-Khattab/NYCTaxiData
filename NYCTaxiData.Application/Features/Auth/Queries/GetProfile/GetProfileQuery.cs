using MediatR;
 using NYCTaxiData.Application.DTOs.Identity; 

namespace NYCTaxiData.Application.Auth.Queries.GetProfile
{

    public record GetProfileQuery(string PhoneNumber) : IRequest<UserResultDto>;
}