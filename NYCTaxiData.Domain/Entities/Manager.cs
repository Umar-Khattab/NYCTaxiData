using System;
using System.Collections.Generic;

namespace NYCTaxiData.Infrastructure;

public partial class Manager
{
    public Guid Id { get; set; }

    public string Employeeid { get; set; } = null!;

    public string? Department { get; set; }

    public virtual User1 IdNavigation { get; set; } = null!;
}
