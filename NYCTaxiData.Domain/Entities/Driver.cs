using NYCTaxiData.Domain.Enums;
using System;
using System.Collections.Generic;

namespace NYCTaxiData.Domain.Entities;

public partial class Driver
{
    public Guid UserId { get; set; }

    public string? FullName { get; set; }

    public string PlateNumber { get; set; } = null!;

    public string LicenseNumber { get; set; } = null!;

    public decimal? Rating { get; set; }
    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();

    public virtual User1 User { get; set; } = null!;
    public CurrentStatus Status { get; set; } = CurrentStatus.Offline;
}
