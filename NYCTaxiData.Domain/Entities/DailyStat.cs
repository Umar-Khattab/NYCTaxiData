using System;
using System.Collections.Generic;

namespace NYCTaxiData.Domain.Entities;

public partial class DailyStat
{
    public int Id { get; set; }

    public DateOnly Date { get; set; }

    public int TotalTrips { get; set; }

    public decimal TotalRevenue { get; set; }

    public int ActiveDrivers { get; set; }

    public double AvgTripMinutes { get; set; }

    public decimal AvgFare { get; set; }

    public int CompletedTrips { get; set; }

    public int CancelledTrips { get; set; }

    public DateTime ComputedAt { get; set; }
}
