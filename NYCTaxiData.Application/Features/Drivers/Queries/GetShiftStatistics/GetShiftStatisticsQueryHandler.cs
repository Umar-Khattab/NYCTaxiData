using MediatR;
using NYCTaxiData.Application.Common.Plumping; // توحيد الـ Namespace للـ Result
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
        // استخدام الـ Entity الجديدة للـ Driver
        var driver = await _unitOfWork.Drivers.GetByIdAsync(request.DriverId);
        if (driver is null)
        {
            return Result<ShiftStatisticsDto>.Failure($"Driver with id '{request.DriverId}' was not found.");
        }

        var shiftEnd = request.ShiftEndUtc ?? DateTime.UtcNow;
        var shiftStart = request.ShiftStartUtc ?? shiftEnd.AddHours(-8);

        // تعديل الـ Query: StartedAt مبقتش Nullable فمش محتاجين HasValue ولا Value
        var trips = (await _unitOfWork.Trips.FindByConditionAsync(t =>
            t.DriverId == request.DriverId
            && t.StartedAt >= shiftStart
            && t.StartedAt <= shiftEnd)).ToList();

        // EndedAt لسه Nullable فبنسيب HasValue زي ما هي
        var completedTrips = trips.Count(t => t.EndedAt.HasValue);

        // تغيير ActualFare لـ TotalAmount بناءً على الـ Entity الجديدة
        var totalEarnings = trips.Sum(t => t.TotalAmount ?? 0m);

        var activeMinutes = trips.Sum(t =>
        {
            var started = t.StartedAt; // نوعها DateTime مباشرة
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
            TotalEarnings = totalEarnings,
            IdleTimeMinutes = idleTimeMinutes
        };

        return Result<ShiftStatisticsDto>.Success(dto, "Shift statistics retrieved successfully");
    }
}