using MediatR;
using NYCTaxiData.Application.Common.Plumbing; //  ÊÕÌœ «·‹ Namespace ··‹ Result
using NYCTaxiData.Domain.Interfaces;
using NYCTaxiData.Infrastructure;

namespace NYCTaxiData.Application.Features.Drivers.Queries.GetShiftStatistics;

public sealed class GetShiftStatisticsQueryHandler : IRequestHandler<GetShiftStatisticsQuery, Result<ShiftStatisticsDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetShiftStatisticsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ShiftStatisticsDto>> Handle(GetShiftStatisticsQuery request, CancellationToken cancellationToken)
    {
        // «” Œœ«„ «·‹ Entity «·ÃœÌœ… ··‹ Driver
        var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);
        if (driver is null)
        {
            return Result<ShiftStatisticsDto>.Failure($"Driver with id '{request.DriverId}' was not found.");
        }

        var shiftEnd = request.ShiftEndUtc ?? DateTime.UtcNow;
        var shiftStart = request.ShiftStartUtc ?? shiftEnd.AddHours(-8);

        //  ⁄œÌ· «·‹ Query: StartedAt „»ﬁ ‘ Nullable ›„‘ „Õ «ÃÌ‰ HasValue Ê·« Value
        var trips = (await _unitOfWork.Trips.FindByConditionAsync(t =>
            t.DriverId == request.DriverId
            && t.StartedAt >= shiftStart
            && t.StartedAt <= shiftEnd)).ToList();

        // EndedAt ·”Â Nullable ›»‰”Ì» HasValue “Ì „« ÂÌ
        var completedTrips = trips.Count(t => t.EndedAt.HasValue);
         

        var activeMinutes = trips.Sum(t =>
        {
            var started = t.StartedAt; // ‰Ê⁄Â« DateTime „»«‘—…
            var ended = t.EndedAt ?? shiftEnd;

            if (ended < shiftStart || started > shiftEnd)
            {
                return 0;
            }

            var boundedStart = started < shiftStart ? shiftStart : started;
            var boundedEnd = ended > shiftEnd ? shiftEnd : ended;

            return (boundedEnd > boundedStart)
            ? (int)((DateTime)boundedEnd - (DateTime)boundedStart).TotalMinutes
            : 0;

        });

        var totalShiftMinutes = (int)Math.Max(0, (shiftEnd - shiftStart).TotalMinutes);
        var idleTimeMinutes = Math.Max(0, totalShiftMinutes - activeMinutes);

        var dto = new ShiftStatisticsDto
        {
            DriverId = request.DriverId,
            ShiftStartUtc = shiftStart,
            ShiftEndUtc = shiftEnd,
            HoursActive = Math.Round(activeMinutes / 60d, 2),
            TripsCompleted = completedTrips, 
            IdleTimeMinutes = idleTimeMinutes
        };

        return Result<ShiftStatisticsDto>.Success(dto, "Shift statistics retrieved successfully");
    }
}