using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumping;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetZoneById
{
    public class GetZoneByIdQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<GetZoneByIdQuery, Result<ZoneDto>>
    {
        public async Task<Result<ZoneDto>> Handle(
            GetZoneByIdQuery request,
            CancellationToken cancellationToken)
        {
            var zone = await _unitOfWork.Zones.Query()
                .AsNoTracking()
                .FirstOrDefaultAsync(z => z.ZoneId == request.ZoneId, cancellationToken);

            if (zone == null)
                return Result<ZoneDto>.Failure($"Zone with ID {request.ZoneId} not found", "NotFound");

            var dto = _mapper.Map<ZoneDto>(zone);
            return Result<ZoneDto>.Success(dto, "Zone retrieved successfully");
        }
    }
}
