using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.Interfaces
{
    public interface IAuditableEntity
    {
        DateTime CreatedAt { get; set; }
        string? CreatedBy { get; set; }
        DateTime? LastUpdatedAt { get; set; }
        string? LastUpdatedBy { get; set; }
    }
}
