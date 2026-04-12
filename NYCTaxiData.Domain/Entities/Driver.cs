using NYCTaxiData.Domain.Enums;
using NYCTaxiData.Infrastructure;
using System;
using System.Collections.Generic;

namespace NYCTaxiData.Domain.Entities;

public partial class Driver
{
    public Guid Id { get; set; }

    public string? Fullname { get; set; }

    public string Platenumber { get; set; } = null!;

    public string Licensenumber { get; set; } = null!;

    public decimal? Rating { get; set; }
    
    public CurrentStatus Status { get; set; }

    public virtual User1 IdNavigation { get; set; } = null!;

    public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
}
