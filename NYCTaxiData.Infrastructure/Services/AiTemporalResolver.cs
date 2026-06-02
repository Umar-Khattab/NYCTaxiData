using System;
using NYCTaxiData.Application.Common.Interfaces;

namespace NYCTaxiData.Infrastructure.Services;

/// <summary>
/// Infrastructure service that aligns absolute dates to the valid AI Database 2024 snapshot.
/// Maps calendar months cyclically to January-April 2024 to preserve seasonal and weekday structures.
/// </summary>
public class AiTemporalResolver : IAiTemporalResolver
{
    private static readonly DateTime SnapshotMin = new DateTime(2024, 01, 01, 0, 0, 0);
    private static readonly DateTime SnapshotMax = new DateTime(2024, 04, 30, 23, 59, 59);

    /// <inheritdoc />
    public DateTime ResolveTemporalContext(DateTime targetTime)
    {
        // If the date is already within the valid snapshot range, use it directly
        if (targetTime >= SnapshotMin && targetTime <= SnapshotMax)
        {
            return targetTime;
        }

        // 1. Resolve Month Cyclically: m = ((M - 1) % 4) + 1
        int targetMonth = targetTime.Month;
        int resolvedMonth = ((targetMonth - 1) % 4) + 1;

        int targetHour = targetTime.Hour;
        int targetMinute = targetTime.Minute;
        int targetSecond = targetTime.Second;
        DayOfWeek targetDOW = targetTime.DayOfWeek;

        // 2. Find a day in 2024 inside the resolved month that shares the exact same Day of Week
        int resolvedDay = 1;
        int daysInMonth = DateTime.DaysInMonth(2024, resolvedMonth);
        for (int day = 1; day <= daysInMonth; day++)
        {
            var candidate = new DateTime(2024, resolvedMonth, day);
            if (candidate.DayOfWeek == targetDOW)
            {
                resolvedDay = day;
                break;
            }
        }

        // Return resolved date keeping hour, minute, and second identical
        return new DateTime(2024, resolvedMonth, resolvedDay, targetHour, targetMinute, targetSecond);
    }

    /// <inheritdoc />
    public DateTime GetLatestSnapshotDate() => SnapshotMax;
}
