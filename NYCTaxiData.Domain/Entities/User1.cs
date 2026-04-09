using NYCTaxiData.Domain.Entities;
using NYCTaxiData.Domain.Enums;
using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure;

public partial class User1
{
    public Guid Id { get; set; }

    public string Firstname { get; set; } = null!;

    public string Lastname { get; set; } = null!;

    public string Phonenumber { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public int? Age { get; set; }

    public string? City { get; set; }

    public string? Street { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Updatedat { get; set; }
    public UserRole Role { get; set; }
    public virtual Driver? Driver { get; set; }

    public virtual Manager? Manager { get; set; }

    public virtual ICollection<Simulationrequest> Simulationrequests { get; set; } = new List<Simulationrequest>();
}
