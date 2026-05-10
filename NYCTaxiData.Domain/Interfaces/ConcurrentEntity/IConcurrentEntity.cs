using System;
using System.Collections.Generic;
using System.Text;

namespace NYCTaxiData.Domain.Interfaces.ConcurrentEntity
{
    public interface IConcurrentEntity
    {
        byte[]? RowVersion { get; set; }
    }
}
