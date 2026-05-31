using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.DTOs.Zone;
using NYCTaxiData.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NYCTaxiData.Application.Features.Zones.Queries.GetAllZones
{
    public class GetAllZonesQueryHandler(IUnitOfWork _unitOfWork, IMapper _mapper)
        : IRequestHandler<GetAllZonesQuery, Result<List<ZoneDto>>>
    {
        public async Task<Result<List<ZoneDto>>> Handle(
            GetAllZonesQuery request,
            CancellationToken cancellationToken)
        {
            var zones = await _unitOfWork.Zones.Query()
                .AsNoTracking()
                .ProjectTo<ZoneDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<List<ZoneDto>>.Success(zones, "Zones retrieved successfully");
        }
    }
}
