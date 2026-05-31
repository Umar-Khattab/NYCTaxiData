using MediatR;
using NYCTaxiData.Application.Common.Plumbing;
using NYCTaxiData.Application.Common.Models;
using NYCTaxiData.Application.Common.Interfaces.MarkerInterfaces;
using NYCTaxiData.Application.DTOs.Identity; // 🚀 أضفنا الـ namespace ده عشان يشوف الـ DriverListDto

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetDriverList;

public sealed record GetDriverListQuery(
    string? Status,
    int? ZoneId,
    int PageNumber = 1,
    int PageSize = 10)
    : IRequest<Result<PaginatedList<DriverListDto>>>, ICacheableQuery; // 👈 حدثناها هنا لـ DriverListDto